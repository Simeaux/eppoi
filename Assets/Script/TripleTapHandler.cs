using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TripleTapHandler : MonoBehaviour, IPointerClickHandler
{
    // Tempo massimo tra un click e l'altro (0.3s è lo standard)
    [SerializeField] float tapThreshold = 0.3f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Controlliamo il numero di click consecutivi rilevati da Unity
        // Il clickCount si resetta automaticamente se passa troppo tempo
        if (eventData.clickCount == 3)
        {
            OnTripleTapDetected();
        }
    }

    void OnTripleTapDetected()
    {
        Debug.Log("Triplo tap rilevato! Apro il menu segreto...");
        // Qui puoi inserire la logica per mostrare il campo del Server URL
    }
}
