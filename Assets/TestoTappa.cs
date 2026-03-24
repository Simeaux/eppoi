using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestoTappa : MonoBehaviour
{
    public TextMesh testo;
    public TextMesh testo2;


    public void SetText(string _testo)
    {
        if (testo != null)
            testo.text = testo.text.Replace("{0}", _testo);
        if (testo2 != null)
            testo2.text = testo.text.Replace("{0}", _testo);
    }

}
