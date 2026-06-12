using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject portalObject; 
    private int totalEnemies;
    private bool enemiesCleared = false;
    private bool puzzleCleared = false;

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        totalEnemies = enemies.Length;

        Debug.Log("Total musuh di level ini: " + totalEnemies);

        if (totalEnemies <= 0) enemiesCleared = true;

        if (portalObject != null)
        {
            portalObject.SetActive(false);
        }
    }

    public void EnemyDefeated()
    {
        totalEnemies--;
        Debug.Log("Sisa musuh: " + totalEnemies);

        if (totalEnemies <= 0)
        {
            enemiesCleared = true;
            Debug.Log("<color=yellow>Syarat 1 Terpenuhi: Semua musuh kalah!</color>");
            CekKondisiPortal();
        }
    }

    public void PuzzleCompleted()
    {
        puzzleCleared = true;
        Debug.Log("<color=yellow>Syarat 2 Terpenuhi: Puzzle Selesai!</color>");
        CekKondisiPortal();
    }

    void CekKondisiPortal()
    {
        if (enemiesCleared && puzzleCleared)
        {
            SpawnPortal();
        }
    }

    void SpawnPortal()
    {
        Debug.Log("<color=green>PORTAL SPAWNED: Musuh mati & Puzzle selesai! Portal aktif!</color>");
        if (portalObject != null)
        {
            portalObject.SetActive(true);
        }
    }
}
