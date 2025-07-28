using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Panel_background_Menu : MonoBehaviour
{
    public Button _btnNFC;

    void Start()
    {
    }
    private void OnEnable()
    {
        _btnNFC.onClick.AddListener(nfc);
    }
    private void OnDisable()
    {
        _btnNFC.onClick.RemoveListener(nfc);
    }

    public void nfc()
    {
        
        
    }

}
