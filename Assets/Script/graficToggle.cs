using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class graficToggle : MonoBehaviour
{
    public Image _img;
    public Image _img_background;
    public Toggle _toggle;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(CambiaColore);
    }
    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(CambiaColore);
    }

    private void CambiaColore(bool _isOn)
    {
        if (_isOn)
        {
            _img.color = Color.white;
            _img_background.gameObject.SetActive(true);
        }
        else
        {
            _img.color = Color.black;
            _img_background.gameObject.SetActive(false);

        }
    }

}
