using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipale : MonoBehaviour
{

    public void ButtonCliccked(Text button)
    {
        PlayerPrefs.SetString("istat", button.text);
        PlayerPrefs.SetString("poi_selezionato", "");
        PlayerPrefs.SetString("percorso_selezionato", "");

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
    }
    public void ButtonClicckedPOI(Text ID)
    {
        PlayerPrefs.SetInt("show_grid_poi", 2);
        PlayerPrefs.SetString("poi_selezionato", ID.text);
    }

}
