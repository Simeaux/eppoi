using UnityEngine;

public class GestoreIndietro : MonoBehaviour
{
    void Update()
    {
        // Intercetta la pressione del tasto Indietro su Android
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

    }
}
