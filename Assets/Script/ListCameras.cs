using UnityEngine;

public class ListCameras : MonoBehaviour
{
    void Start()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            Debug.Log("CAMERA: " + cam.name);
        }
    }
}