using UnityEngine;
using UnityEngine.UI;

public class ShowTextureBuffer : MonoBehaviour
{
    public Camera textureBufferCamera;
    public RawImage rawImage;

    void Update()
    {
        if (rawImage.texture == null &&
            textureBufferCamera != null &&
            textureBufferCamera.targetTexture != null)
        {
            rawImage.texture = textureBufferCamera.targetTexture;
        }
    }
}