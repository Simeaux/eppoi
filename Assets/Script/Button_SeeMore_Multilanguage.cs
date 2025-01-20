using UnityEngine;
using UnityEngine.UI;

public class Button_SeeMore_Multilanguage : MonoBehaviour
{
    [SerializeField]
    private Text text;

    void Start()
    {
        text.text = PlayerPrefs.GetInt("lingua_selezionata") == 1 ? "Vedi di più" : "See More";
    }

}
