using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasLoader : MonoBehaviour
{
    public Text lblCaricamento;

    private int _lingua_selezionata = 1;
    // Start is called before the first frame update
    void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        lblCaricamento.text = _lingua_selezionata == 1 ? "Caricamento ..." : "Loading ...";
    }

}
