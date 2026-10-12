using UnityEngine;
using UnityEngine.UI;

public class ButtonTrigger : MonoBehaviour
{
    // Trascina qui il secondo bottone dall'Inspector
    public Button secondoBottone;

    public void CliccaAltroBottone()
    {
        if (secondoBottone != null)
        {
            secondoBottone.onClick.Invoke();
        }
    }
}
