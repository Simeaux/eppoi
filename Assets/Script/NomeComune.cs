using System.Collections.Generic;
using UnityEngine;

public class NomeComune : MonoBehaviour
{
    public List<TextMesh> testo;


    public void SetText(string _testo)
    {
        foreach (var _t in testo)
            _t.text = _t.text.Replace("{0}", _testo);
    }
}
