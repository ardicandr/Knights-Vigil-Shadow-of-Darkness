using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour {
    [Header("Settings")]
    public bool isAttacking = false;
    public float damage = 25f;
    public float hitRadius = 0.2f; // Kapak Brute mungkin butuh radius lebih besar (0.3)
    public LayerMask enemyLayer;
    public Transform[] rayPoints;

    private List<GameObject> hitTargets = new List<GameObject>();
    private Dictionary<Transform, Vector3> lastPointPositions = new Dictionary<Transform, Vector3>();

    void Update() {
        if (isAttacking) {
            CalculateSweep();
        } else {
            lastPointPositions.Clear();
            hitTargets.Clear();
        }
    }

    void CalculateSweep() {
        foreach (Transform point in rayPoints) {
            if (!lastPointPositions.ContainsKey(point)) {
                lastPointPositions[point] = point.position;
                continue;
            }

            Vector3 startPos = lastPointPositions[point];
            Vector3 endPos = point.position;
            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;

            // HANYA deteksi jika ada pergerakan nyata (mencegah bug damage saat diam)
            if (distance > 0.01f) {
                RaycastHit[] hits = Physics.SphereCastAll(startPos, hitRadius, direction.normalized, distance, enemyLayer);

                foreach (RaycastHit hit in hits) {
                    GameObject target = hit.collider.gameObject;

                    // Jangan memukul diri sendiri
                    if (target.transform.root == transform.root) continue;

                    if (!hitTargets.Contains(target)) {
                        hitTargets.Add(target);
                        ApplyDamage(target);
                    }
                }
            }
            lastPointPositions[point] = endPos;
        }
    }

    void ApplyDamage(GameObject target) {
        // SERANGAN PALADIN KE BRUTE
        EnemyHealth eHealth = target.GetComponentInParent<EnemyHealth>();
        if (eHealth != null) {
            eHealth.TakeDamage(damage);
            Debug.Log("<color=green>SWORD HIT:</color> " + target.name);
            return; // Keluar agar tidak mengecek player health
        }

        // SERANGAN BRUTE KE PALADIN
        PlayerHealth pHealth = target.GetComponentInParent<PlayerHealth>();
        if (pHealth != null && !pHealth.isDead) {
            pHealth.TakeDamage(damage);
            Debug.Log("<color=red>AXE HIT PLAYER!</color>");
        }
    }

    public void ClearHitList() {
        hitTargets.Clear();
        lastPointPositions.Clear();
    }
}