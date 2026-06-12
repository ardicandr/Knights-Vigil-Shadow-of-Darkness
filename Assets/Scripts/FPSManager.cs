using UnityEngine;

public class FPSManager : MonoBehaviour
{
    private static FPSManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SetFPS60();
    }

    void SetFPS60()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Debug.Log("FPS dikunci ke 60 FPS");
    }
}
