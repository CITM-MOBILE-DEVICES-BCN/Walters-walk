using UnityEngine;
using UnityEngine.UI;

public class ButtonColorSetter : MonoBehaviour
{
    [SerializeField] private GlobalColorManager colorManager; // Reference to the global manager
    [SerializeField] private Color assignedColor; // Color assigned to this button
    [SerializeField] private Button button; // The button itself

    void Start()
    {
        // Add a listener for button click
        button.onClick.AddListener(SetGlobalColor);
    }

    private void SetGlobalColor()
    {
        // Set the global color to this button's assigned color
        colorManager.selectedColor = assignedColor;
    }

    private void OnDestroy()
    {
        // Remove the listener to avoid memory leaks
        button.onClick.RemoveListener(SetGlobalColor);
    }
}
