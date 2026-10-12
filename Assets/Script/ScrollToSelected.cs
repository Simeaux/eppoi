using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform viewportRectTransform;
    private GameObject lastSelected;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewportRectTransform = scrollRect.viewport;
    }

    void Update()
    {
        // Controlla l'elemento attualmente selezionato dall'EventSystem
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null) return;
        if (currentSelected == lastSelected) return;

        // Verifica che l'elemento selezionato sia figlio del Content dello ScrollRect
        if (currentSelected.transform.IsChildOf(scrollRect.content))
        {
            RectTransform selectedRectTransform = currentSelected.GetComponent<RectTransform>();
            if (selectedRectTransform != null)
            {
                UpdateScrollPosition(selectedRectTransform);
            }
        }

        lastSelected = currentSelected;
    }

    void UpdateScrollPosition(RectTransform target)
    {
        // Calcola la posizione locale del target rispetto al content
        Vector3 targetLocalPos = scrollRect.content.InverseTransformPoint(target.position);
        // Calcola la posizione del viewport rispetto al content
        Vector3 viewportLocalPos = scrollRect.content.InverseTransformPoint(viewportRectTransform.position);

        // Calcola la differenza
        Vector3 delta = targetLocalPos - viewportLocalPos;

        // Se lo scroll è verticale, muovi l'asse Y
        if (scrollRect.vertical)
        {
            Vector2 contentPos = scrollRect.content.anchoredPosition;
            contentPos.y -= delta.y;
            scrollRect.content.anchoredPosition = contentPos;
        }

        // Se lo scroll è orizzontale, muovi l'asse X
        if (scrollRect.horizontal)
        {
            Vector2 contentPos = scrollRect.content.anchoredPosition;
            contentPos.x -= delta.x;
            scrollRect.content.anchoredPosition = contentPos;
        }
    }
}
