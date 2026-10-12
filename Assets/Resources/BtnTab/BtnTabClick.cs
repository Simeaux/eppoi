using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnTabClick : MonoBehaviour
{
    public Button _btn;
    public bool _clicked = false;
    public Image _bck_img;
    public Image _img;
    public Text _text;

    private void Start()
    {
        changes();
    }

    private void changes()
    {
        if (_img != null)
        {
            //_img.gameObject.SetActive(_clicked);
            Color c = new Color();
            Color bck = new Color();
            c = Color.black;
            bck = Color.white;
            if (!_btn.enabled)
                c = Color.grey;
            else
            {
                c = Color.black;
                if (_clicked)
                {
                    Color _c = new Color();
                    if (ColorUtility.TryParseHtmlString("#ffffff", out _c))
                        c = _c;
                    bck = Color.black;
                }
            }
            _text.color = c;
            _img.color = c;
            _bck_img.color = bck;
            _text.fontStyle = _clicked ? FontStyle.Bold : FontStyle.Normal;
            //_btn.gameObject.SetActive(!_clicked);
            _btn.gameObject.GetComponentInChildren<Image>().color = c;
        }
    }
    public void SetCliccked(bool _click)
    {
        _clicked = _click;
        changes();
    }
}
