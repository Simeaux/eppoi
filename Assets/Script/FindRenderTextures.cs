using UnityEngine;

public class FindRenderTextures : MonoBehaviour
{
    void Start()
    {
        foreach (RenderTexture rt in Resources.FindObjectsOfTypeAll<RenderTexture>())
        {
            Debug.Log(
                "RT: " +
                rt.name +
                " " +
                rt.width +
                "x" +
                rt.height
            );
        }
    }
}