using UnityEngine;
using Vuforia;

public class ForceVuforiaVideoShader : MonoBehaviour
{
    void Awake()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += ForceShader;
    }

    void ForceShader()
    {
        GameObject videoBackground = GameObject.Find("VideoBackground");

        if (videoBackground == null)
        {
            Debug.LogError("VideoBackground GameObject non trovato");
            return;
        }

        Renderer renderer = videoBackground.GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.LogError("Renderer del VideoBackground non trovato");
            return;
        }

        Shader shader = Shader.Find("Vuforia/Built-In/VideoBackground");

        if (shader == null)
        {
            Debug.LogError("Shader Vuforia/Built-In/VideoBackground non trovato");
            return;
        }

        renderer.material.shader = shader;

        Debug.Log("Shader forzato: " + shader.name);
    }
}