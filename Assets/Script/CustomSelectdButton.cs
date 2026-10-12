using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomSelectdButton : MonoBehaviour
{
    [Header("Elementi UI")]
    [SerializeField] private Image background;
    [SerializeField] private Text textGraphic;
    [SerializeField] private bool isSelected;

    [Header("Colori")]
    [SerializeField] private Color normalBackground = Color.white;
    [SerializeField] private Color normalText = Color.black;
    [SerializeField] private Color selectedBackground = Color.black;
    [SerializeField] private Color selectedText = Color.white;



    private void Awake()
    {
        if (background == null)
            background = GetComponent<Image>();

        ApplyColors();
    }

    // Da collegare all'evento On Click() del Button
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        ApplyColors();
    }

    private void ApplyColors()
    {
        if (background != null)
            background.color = isSelected
                ? selectedBackground
                : normalBackground;

        if (textGraphic != null)
            textGraphic.color = isSelected
                ? selectedText
                : normalText;
    }
}
