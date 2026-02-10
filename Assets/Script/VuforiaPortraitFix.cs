using UnityEngine;
using Vuforia;

public class VuforiaPortraitFix : MonoBehaviour
{
    void Awake()
    {
        // Forza l'orientamento di Unity in verticale
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Start()
    {
        // Registra l'evento per quando Vuforia è pronto
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }

    void OnVuforiaStarted()
    {
        // Forza Vuforia a ricalcolare le dimensioni del background video
        // basandosi sull'orientamento Portrait attuale
        var vb = VuforiaBehaviour.Instance.VideoBackground;
        if (vb != null)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Debug.Log("Rotazione camera resettata in Portrait.");
        }

    }
}
