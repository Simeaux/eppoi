using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleColorController : MonoBehaviour
{
    private Toggle toggle;

    [Header("Componenti Grafici")]
    public Image sfondo;
    public Image icona;
    public Text testo;

    [Header("Colori Stato Deselezionato")]
    public Color sfondoNormale = Color.white;
    public Color iconaNormale = Color.gray;
    public Color testoNormale = Color.black;

    [Header("Colori Stato Selezionato")]
    public Color sfondoSelezionato = Color.blue;
    public Color iconaSelezionata = Color.white;
    public Color testoSelezionato = Color.blue;


    public Text gruppo_tipo_selezionato;
    public int gruppo_tipo_selezionato_valore = 0;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        // Ascolta il cambio di stato del toggle
        toggle.onValueChanged.AddListener(AggiornaGrafica);
    }

    void Start()
    {
        // Imposta lo stato iniziale coerente
        AggiornaGrafica(toggle.isOn);
    }


    public void AggiornaGrafica(bool selezionato)
    {
        if (selezionato)
        {
            sfondo.color = sfondoSelezionato;
            icona.color = iconaSelezionata;
            testo.color = testoSelezionato;
            if (gruppo_tipo_selezionato != null)
                gruppo_tipo_selezionato.text = gruppo_tipo_selezionato_valore.ToString();
        }
        else
        {
            sfondo.color = sfondoNormale;
            icona.color = iconaNormale;
            testo.color = testoNormale;
            if (gruppo_tipo_selezionato != null)
                gruppo_tipo_selezionato.text = "0";
        }
    }

    void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(AggiornaGrafica);
    }
}
