using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelFiltro : MonoBehaviour
{
    public Text txtSearch_Placeholder;
    public GameObject btn_activate_Search;
    // Start is called before the first frame update

    void Start()
    {
        var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        txtSearch_Placeholder.text = _lingua_selezionata == 1 ? "Dove andare? Cosa fare?" : "Where to go? What to do?";
        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();
        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName == "Map")
        {
            btn_activate_Search.SetActive(true);
        }
        else
            btn_activate_Search.SetActive(false);
    }

    
}
