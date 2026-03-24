using UnityEngine;
using UnityEngine.UI;

public class AutoRectMap : MonoBehaviour
{
    public RawImage mappaUI;    // Trascina la Raw Image della ScrollView
    public RenderTexture mappaRT; // Trascina il file della Render Texture

    void Awake()
    {
        // 1. Recuperiamo la larghezza effettiva della UI sullo schermo attuale
        RectTransform rt = mappaUI.GetComponent<RectTransform>();

        // Forza l'aggiornamento del layout per essere sicuri di avere i dati corretti
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        int larghezzaDinamica = (int)rt.rect.width;
        int altezzaFissa = 400;

        // 2. Riconfiguriamo la Render Texture per farla diventare rettangolare
        mappaRT.Release(); // Svuota la vecchia configurazione (1024x1024)
        mappaRT.width = larghezzaDinamica;
        mappaRT.height = altezzaFissa;
        mappaRT.Create(); // La ricrea con le proporzioni del telefono

        Debug.Log($"Render Texture impostata a: {larghezzaDinamica}x{altezzaFissa}");
    }
}
