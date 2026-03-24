using UnityEngine;
using UnityEngine.UI;

public class PanelBrowser : MonoBehaviour
{

    public Image _logo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _logo.transform.Rotate(0, 0, -0.4f);
    }
}
