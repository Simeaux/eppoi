using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestoTappa : MonoBehaviour
{
    public TextMesh testo;


    public void SetText(string _testo)
    {
        testo.text = testo.text.Replace("{0}", _testo);
    }

}
