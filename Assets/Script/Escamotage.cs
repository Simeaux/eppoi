using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escamotage : MonoBehaviour
{
    public List<GameObject> _panel;
    public GameObject saveme;

    // Update is called once per frame
    void Update()
    {
        if (_panel.FindAll(o => o.activeSelf).Count > 0)
        {
            if (saveme.activeSelf)
                saveme.SetActive(false);
        }
        else
        {
            if (!saveme.activeSelf)
                saveme.SetActive(true);
        }
    }
}
