using UnityEngine;
using UnityEngine.EventSystems;

public class TestClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Stampa il numero di click ogni volta che tocchi
        Debug.Log("Click rilevati: " + eventData.clickCount);

        if (eventData.clickCount == 3)
        {
            Debug.Log("TRIPLO TAP RIUSCITO!");
        }
    }
}
