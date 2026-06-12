using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public Animator anim;
    public NavMeshAgent agent;

    [Header("Audio Settings")]
    public AudioSource bruteAudioSource;
    public AudioClip hitSFX;
    public AudioClip deathSFX;
    public AudioClip fallSFX;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (bruteAudioSource != null && hitSFX != null)
            bruteAudioSource.PlayOneShot(hitSFX);

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    void Die() {
        if (isDead) return;
        isDead = true;

        if (bruteAudioSource != null && deathSFX != null)
            bruteAudioSource.PlayOneShot(deathSFX);

        anim.SetBool("isDead", true);

        // --- Logika AI Mati ---
        BruteAI aiScript = GetComponent<BruteAI>();
        if (aiScript != null && aiScript.axeWeapon != null)
        {
            aiScript.axeWeapon.isAttacking = false;
            aiScript.axeWeapon.ClearHitList();
        }

        if (agent != null) {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        LevelManager manager = FindObjectOfType<LevelManager>();
        if (manager != null) manager.EnemyDefeated();
    }

    public void PlayBodyFallSFX()
    {
        if (bruteAudioSource != null && fallSFX != null)
        {
            bruteAudioSource.PlayOneShot(fallSFX, 1.0f); 
            Debug.Log("SFX: Brute menghantam tanah.");
        }
    }
}