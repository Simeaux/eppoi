using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipale : MonoBehaviour
{

    private DBClass _DBClass;
    private Canvas _Canvas_Prompt_Download;
    public Panel_Principale pp;
    public void ButtonCliccked(Button button)
    {
        //var NomeComune = button.transform.Find("NomeComune").GetComponent<Text>().text.Replace(" (MC)", "");
        var Istat = button.transform.Find("Istat").GetComponent<Text>().text;
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        List<DBClass.COMUNE> c = _DBClass.GetCOMUNI(Istat, null, null, null, null);
        if (c != null && c.Count > 0)
        {
            int _TotalRowToExtract = _DBClass.getPOI_Count(null, null, c[0].nome_comune, null, null);
            if (_TotalRowToExtract > 0)
            {
                PlayerPrefs.SetString("istat", Istat);
                PlayerPrefs.SetString("poi_selezionato", "");
                PlayerPrefs.SetString("percorso_selezionato", "");

            }
            else
            {
                //qui devo far partire il download dei dati del comune
                if (pp == null)
                {
                    pp = GameObject.FindFirstObjectByType<Panel_Principale>();
                }

                // Controllo di sicurezza: verifichiamo che il pannello esista effettivamente nella scena
                if (pp != null)
                {
                    // La sync ora e' una coroutine (spinner + scrittura DB a blocchi + reload maschera).
                    // La avviamo tramite un wrapper che aspetta la fine e poi apre il comune se ha i POI.
                    StartCoroutine(DownloadEApriComune(c[0], button));
                }
                else
                {
                    Debug.LogError("Errore: Impossibile trovare Panel_Principale nella scena!");
                }
            }
        }

    }



    // Avvia il prompt, attende la risposta dell'utente, esegue il download e apre il comune.
    private IEnumerator DownloadEApriComune(DBClass.COMUNE comune, Button button)
    {

        _Canvas_Prompt_Download = pp.gameObject.transform.Find("Canvas_Prompt_Download").GetComponent<Canvas>();
        // 1. Mostriamo il Canvas di Prompt
        if (_Canvas_Prompt_Download != null)
        {
            _Canvas_Prompt_Download.gameObject.SetActive(true);

            // Otteniamo il componente dello script associato al prompt per leggerne il risultato
            PromptDownloadController promptScript = _Canvas_Prompt_Download.GetComponent<PromptDownloadController>();
            if (promptScript != null)
            {
                // Reset dello stato iniziale prima di attendere
                promptScript.InizializzaPrompt();

                // Blocco yield: la coroutine si ferma qui FINCHÉ l'utente non clicca uno dei due bottoni
                yield return new WaitUntil(() => promptScript.RispostaRicevuta == true);

                // Verifichiamo il risultato scelto dall'utente
                if (promptScript.RisultatoScelta == false)
                {
                    Debug.Log("Download annullato dall'utente.");
                    _Canvas_Prompt_Download.gameObject.SetActive(false); // Chiudiamo il prompt
                    yield break; // Interrompe definitivamente la coroutine senza scaricare nulla
                }
            }
        }

        // 2. Se l'utente ha premuto SI (oppure se il Canvas non era presente), procediamo con il download
        var Istat = button.transform.Find("Istat").GetComponent<Text>().text;
        List<int> idSinp = new List<int> { comune.id };
        yield return StartCoroutine(pp.StartSync(idSinp, button));

        Debug.Log("Download completato con successo!");
        pp.EndSync(button);
        _Canvas_Prompt_Download.gameObject.SetActive(false); // Chiudiamo il prompt
        int poiCount = _DBClass.getPOI_Count(null, null, comune.nome_comune, null, null);
        if (poiCount > 0)
        {
            PlayerPrefs.SetString("istat", Istat);
            PlayerPrefs.SetString("poi_selezionato", "");
            PlayerPrefs.SetString("percorso_selezionato", "");
        }
    }


    public void ButtonClicckedComune(Text text)
    {
        if (text != null && text.text.Contains(";"))
        {
            var listText = text.text.Split(';');
            if (listText != null)
            {
                // Create a temporary reference to the current scene.
                Scene currentScene = SceneManager.GetActiveScene();
                // Retrieve the name of this scene.
                string sceneName = currentScene.name;
                int tipo = int.Parse(listText[0]);


                long id = long.Parse(listText[1]);
                if (tipo == 1 && !(PlayerPrefs.GetString("poi_selezionato") != ""))
                {
                    var _percorso = GameObject.FindObjectOfType<DBClass>().GetPERCORSO(id);
                    if (_percorso != null)
                    {
                        var _poi = GameObject.FindObjectOfType<DBClass>().getPOI(_percorso[0].poi_id);
                        if (_poi != null && _poi.Count > 0)
                        {

                            var _comune = GameObject.FindObjectOfType<DBClass>().GetCOMUNI(null, null, _poi[0].comune_id);
                            if (_comune != null && _comune.Count > 0)
                                PlayerPrefs.SetString("istat", _comune[0].istat);
                        }
                    }

                    PlayerPrefs.SetString("percorso_selezionato", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                else if (tipo == 1 && (PlayerPrefs.GetString("poi_selezionato") != ""))
                {
                    PlayerPrefs.SetString("percorso_selezionato_collegato_ad_un_poi", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }

                if (tipo == 2)
                {
                    PlayerPrefs.SetString("evento_selezionato", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                if (tipo == 3 && !(PlayerPrefs.GetString("percorso_selezionato") != ""))
                {
                    var _comune = GameObject.FindObjectOfType<DBClass>().GetCOMUNI(null, null, GameObject.FindObjectOfType<DBClass>().getPOI(id)?[0]?.comune_id);
                    if (_comune != null && _comune.Count > 0)
                        PlayerPrefs.SetString("istat", _comune[0].istat);
                    PlayerPrefs.SetString("poi_selezionato", long.Parse(listText[1]).ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                else if (tipo == 3 && (PlayerPrefs.GetString("percorso_selezionato") != ""))
                {
                    PlayerPrefs.SetString("poi_selezionato_collegato_ad_un_percorso", listText[1]);
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }

            }
        }
        else
        {
            Application.OpenURL(text.text);
        }
    }
    public void ButtonClicckedPOI(Text ID)
    {
        PlayerPrefs.SetInt("show_grid_poi", 2);
        PlayerPrefs.SetString("poi_selezionato", ID.text);
    }

}
