using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnTabClick : MonoBehaviour
{
    public Button _btn;
    public bool _clicked = false;
    public Image _img;
    public Text _text;
    public GameObject _browserPanel;
    public GameObject _webPanel;
    private void Start()
    {
        changes();
    }

    private void changes()
    {
        _img.gameObject.SetActive(_clicked);
        Color c = new Color();
        c = Color.black;
        if (!_btn.enabled)
            c = Color.grey;
        else
        {
            c = Color.black;
            if (_clicked)
            {
                Color _c = new Color();
                if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                    c = _c;
                if (_browserPanel != null)
                    _browserPanel.SetActive(true);
                if (_webPanel != null)
                    _webPanel.SetActive(true);

            }
            else
            {
                if (_browserPanel != null)
                    _browserPanel.SetActive(false);
                if (_webPanel != null)
                    _webPanel.SetActive(false);

            }
        }
        _text.color = c;
        _img.color = c;
        _text.fontStyle = _clicked ? FontStyle.Bold : FontStyle.Normal;
        //_btn.gameObject.SetActive(!_clicked);
        _btn.gameObject.GetComponentInChildren<Image>().color = c;
    }
    public void SetCliccked(bool _click)
    {
        _clicked = _click;
        changes();
    }
}
