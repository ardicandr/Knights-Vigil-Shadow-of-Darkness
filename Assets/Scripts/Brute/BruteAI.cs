using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BruteAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player; 
    public Animator anim;
    public WeaponSystem axeWeapon;

    [Header("AI Settings")]
    public float chaseRange = 15f;
    public float viewAngle = 120f;
    public float attackRange = 2.5f; 
    public float attackCooldown = 3f;
    private float lastAttackTime;

    [Header("Roaming Settings")]
    public float roamRadius = 15f;    
    public float roamWaitTime = 3f;  
    private float roamTimer;

    [Header("Stuck Detection")]
    private float stuckCheckTimer;
    public float stuckThreshold = 2f; 

    [Header("Audio Settings")]
    public AudioSource bruteAudioSource;
    public AudioClip axeSwingSFX;
    [Tooltip("Masukkan beberapa variasi suara erangan/teriakan musuh saat mengejar player")]
    [SerializeField] private AudioClip[] chaseGrowlSounds;
    [Tooltip("Jeda waktu minimal antar suara saat mengejar (detik)")]
    [SerializeField] private float minChaseAudioDelay = 4f;
    [Tooltip("Jeda waktu maksimal antar suara saat mengejar (detik)")]
    [SerializeField] private float maxChaseAudioDelay = 8f;

    private PlayerHealth playerHealth;
    private bool isChasing = false;
    private float chaseAudioTimer;
    private float nextChaseAudioDelay; 
    private int lastPlayedAudioIndex = -1;

    void Start() {
        if (player != null) {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        InvokeRepeating("UpdatePath", 0f, 0.5f); 

        ResetChaseAudioTimer();
    }

    void Update()
    {
        if (PauseMenuController.isPaused) return;
        if (anim.GetBool("isDead")) return;

        bool isPlayerDead = (playerHealth != null && playerHealth.currentHealth <= 0);

        if (player == null || isPlayerDead)
        {
            if (isChasing)
            {
                isChasing = false;
                agent.ResetPath();
                roamTimer = roamWaitTime;
            }
            
            StartRoaming();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isChasing)
        {
            if (CanSeePlayer()) 
            {
                isChasing = true;
                ResetChaseAudioTimer();
            }
            else 
            {
                StartRoaming();
            }
        }
        else
        {
            if (distanceToPlayer > chaseRange) 
            {
                isChasing = false;
            }

            HandleChaseAudio();

            if (distanceToPlayer <= attackRange)
            {
                Attack();
            }
            else
            {
                Chase();
            }
        }
    }

    void HandleChaseAudio()
    {
        if (chaseGrowlSounds == null || chaseGrowlSounds.Length == 0 || bruteAudioSource == null) return;

        chaseAudioTimer += Time.deltaTime;

        if (chaseAudioTimer >= nextChaseAudioDelay)
        {
 
            if (!bruteAudioSource.isPlaying)
            {
                PlayRandomChaseSound();
            }
            ResetChaseAudioTimer();
        }
    }

    void PlayRandomChaseSound()
    {
        int randomIndex = 0;

        if (chaseGrowlSounds.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, chaseGrowlSounds.Length);
            } while (randomIndex == lastPlayedAudioIndex);
        }

        lastPlayedAudioIndex = randomIndex;
        AudioClip clipToPlay = chaseGrowlSounds[randomIndex];

        if (clipToPlay != null)
        {
            bruteAudioSource.pitch = Random.Range(0.9f, 1.1f);
            bruteAudioSource.PlayOneShot(clipToPlay);
        }
    }

    void ResetChaseAudioTimer()
    {
        chaseAudioTimer = 0f;
        nextChaseAudioDelay = Random.Range(minChaseAudioDelay, maxChaseAudioDelay);
    }

    bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > chaseRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle <= viewAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, chaseRange))
            {
                if (hit.collider.CompareTag("Player")) return true;
            }
        }
        return false;
    }

    void UpdatePath() {
        bool isPlayerDead = (playerHealth != null && playerHealth.currentHealth <= 0);
        if (isChasing && player != null && !isPlayerDead && !anim.GetBool("isDead") && agent.enabled) {
            agent.SetDestination(player.position);
        }
    }

    void Chase() {
        agent.isStopped = false;
        anim.SetFloat("Speed", agent.velocity.magnitude / agent.speed, 0.1f, Time.deltaTime);
    }

    void Attack() {
        agent.isStopped = true;
        anim.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; 
        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (Time.time >= lastAttackTime + attackCooldown) {
            anim.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    void StartRoaming()
    {
        if (!agent.enabled) return;
        agent.isStopped = false;
        
        float speed = agent.velocity.magnitude / agent.speed;
        anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        if (speed < 0.1f) stuckCheckTimer += Time.deltaTime;
        else stuckCheckTimer = 0;

        if ((!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) || stuckCheckTimer >= stuckThreshold)
        {
            roamTimer += Time.deltaTime;
            if (roamTimer >= roamWaitTime || stuckCheckTimer >= stuckThreshold)
            {
                Vector3 newDest = GetRandomRoamPoint(transform.position, roamRadius);
                agent.SetDestination(newDest);
                roamTimer = 0;
                stuckCheckTimer = 0; 
                Debug.Log("Brute meninggalkan mayat/lokasi terakhir dan mencari titik baru.");
            }
        }
    }

    Vector3 GetRandomRoamPoint(Vector3 center, float distance)
    {
        for (int i = 0; i < 5; i++) 
        {
            Vector3 randomDir = Random.insideUnitSphere * distance;
            randomDir += center;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, distance, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(hit.position, path);
                if (path.status == NavMeshPathStatus.PathComplete) return hit.position;
            }
        }
        return center; 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 leftRayDirection = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, leftRayDirection * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up, rightRayDirection * chaseRange);
    }

    public void StartAttack() { 
        if (axeWeapon != null) axeWeapon.isAttacking = true; 

        if (bruteAudioSource != null && axeSwingSFX != null)
            bruteAudioSource.PlayOneShot(axeSwingSFX);
    }
    public void StopAttack() { if (axeWeapon != null) { axeWeapon.isAttacking = false; axeWeapon.ClearHitList(); } }
}