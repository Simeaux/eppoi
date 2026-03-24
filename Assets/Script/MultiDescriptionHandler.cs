using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class MultiDescriptionHandler : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text m_TextComponent;
    public ScrollRect scrollRect; // trascina qui lo ScrollRect dall'Inspector

    // Dizionario con le descrizioni estese abbinate agli ID

    void Awake() => m_TextComponent = GetComponent<TMP_Text>();

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = m_TextComponent.textInfo.linkInfo[linkIndex];
            string id = linkInfo.GetLinkID();

            if (id.Contains("poi_"))
            {
                PlayerPrefs.SetString("poi_selezionato_collegato_ad_un_percorso", id.Replace("poi_", ""));
            }
            else if (id.Contains("tappa_"))
            {
                ScrollToLinkTarget(id);
            }
            else
            {
                // Sostituisce il tag [Altro] con la descrizione reale
                string tagDaCercare = "<link=" + id + "><color=#0000EE>[Altro]</color></link><size=0% id=" + id + ">";
                m_TextComponent.text = m_TextComponent.text.Replace(tagDaCercare, "<size=100%>");
            }
        }
    }
    void ScrollToLinkTarget(string targetID)
    {
        // Cerchiamo nel testo il link che funge da "ancora"
        for (int i = 0; i < m_TextComponent.textInfo.linkCount; i++)
        {
            var info = m_TextComponent.textInfo.linkInfo[i];
            if (info.GetLinkID() == "ancora_" + targetID)
            {
                // Trovato! Calcoliamo la posizione del primo carattere del link
                int charIndex = info.linkTextfirstCharacterIndex;
                float charY = m_TextComponent.textInfo.characterInfo[charIndex].baseLine;

                // Calcolo della posizione normalizzata (0 = fondo, 1 = cima)
                float textHeight = m_TextComponent.rectTransform.rect.height;
                float viewportHeight = scrollRect.viewport.rect.height;

                // Rapporto della posizione rispetto all'altezza totale
                float normalizedPos = 1f - (Mathf.Abs(charY) / (textHeight - viewportHeight));
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
                break;
            }
        }
    }
}
