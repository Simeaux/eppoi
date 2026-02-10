using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class CameraToRawImage : MonoBehaviour
{
    public RawImage targetRawImage;

    void Awake()
    {
        // Ci iscriviamo all'evento di avvio di Vuforia
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }
    void Start()
    {
        // Chiama l'inizializzazione manuale
        VuforiaApplication.Instance.Initialize();
    }

    void OnVuforiaStarted()
    {
        // Ci iscriviamo all'evento che indica che la texture del video è pronta
        VuforiaBehaviour.Instance.VideoBackground.OnVideoBackgroundChanged += OnVideoBackgroundChanged;
    }
    // Dentro OnEnable()
    void OnEnable()
    {
        // Forza Vuforia a partire se era fermo
        VuforiaBehaviour.Instance.enabled = true;
    }
    void OnVideoBackgroundChanged()
    {
        // Accediamo alla texture corretta tramite il VideoBackground della ARCamera
        var videoTexture = VuforiaBehaviour.Instance.VideoBackground.VideoBackgroundTexture;

        if (videoTexture != null && targetRawImage != null)
        {
            targetRawImage.texture = videoTexture;
        }
    }
}
