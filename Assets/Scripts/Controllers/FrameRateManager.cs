using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = 60;
    }
}