using UnityEngine;
using UnityEngine.UI;

public class resetText : MonoBehaviour
{
    public void reset_testo(Text testo)
    {
        testo.text = string.Empty;
    }
}
