using UnityEngine;
using UnityEngine.UI;

public class ButtonColorApplier : MonoBehaviour
{
    [SerializeField] private GlobalColorManager colorManager; // Reference to the global manager
    [SerializeField] private Button[] buttons; // Buttons to apply the global color

    void Start()
    {
        ApplyGlobalColor();
    }

    private void ApplyGlobalColor()
    {
        foreach (Button button in buttons)
        {
            var buttonColors = button.colors;
            buttonColors.normalColor = colorManager.selectedColor;
            buttonColors.highlightedColor = colorManager.selectedColor * 1.2f; // Slightly brighter for highlight
            buttonColors.pressedColor = colorManager.selectedColor * 0.8f; // Slightly darker for press
            button.colors = buttonColors;
        }
    }
}
