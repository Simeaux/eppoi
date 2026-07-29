using UnityEngine;
using UnityEngine.UI;

public class button_comune : MonoBehaviour
{

    [SerializeField]
    public Image image_to_download;
    public Image image_downloaded;
    public Text NomeComune;

    private DBClass _DBClass;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        int _TotalRowToExtract = _DBClass.getPOI_Count(null, null, NomeComune.text.Replace(" (MC)", ""), null, null);
        if (_TotalRowToExtract > 0 && image_to_download.gameObject.activeSelf)
        {
            if (image_to_download != null)
                image_to_download.gameObject.SetActive(false);
            if (image_downloaded != null)
                image_downloaded.gameObject.SetActive(true);
        }
        else if (!image_to_download.gameObject.activeSelf)
        {
            if (image_to_download != null)
                image_to_download.gameObject.SetActive(true);
            if (image_downloaded != null)
                image_downloaded.gameObject.SetActive(false);
        }
    }
}
