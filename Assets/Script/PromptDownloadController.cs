using UnityEngine;
using UnityEngine.UI;

public class PromptDownloadController : MonoBehaviour
{
    public Slider Slider_download;

    public Button Button_Si;
    public Button Button_No;

    // Proprietà lette dalla Coroutine del Menu Principale
    public bool RispostaRicevuta { get; private set; }
    public bool RisultatoScelta { get; private set; }

    // Resetta lo stato ogni volta che viene aperto un nuovo prompt
    public void InizializzaPrompt()
    {
        RispostaRicevuta = false;
        RisultatoScelta = false;
        Button_Si.interactable = true;
        Button_No.interactable = true;
    }

    // Collega questo metodo al componente Button -> OnClick() del bottone "SÌ" nell'Inspector
    public void PremutoSi()
    {
        RisultatoScelta = true;
        RispostaRicevuta = true;
        Slider_download.gameObject.SetActive(true);
        Button_Si.interactable = false;
        Button_No.interactable = false;
        //gameObject.SetActive(false); // Chiude il canvas del prompt
    }

    // Collega questo metodo al componente Button -> OnClick() del bottone "NO" nell'Inspector
    public void PremutoNo()
    {
        RisultatoScelta = false;
        RispostaRicevuta = true;
        gameObject.SetActive(false); // Chiude il canvas del prompt
    }
}
