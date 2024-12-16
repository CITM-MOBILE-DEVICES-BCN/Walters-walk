using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for 2D Lights

public class TorchLightFlicker : MonoBehaviour
{
    private Light2D torchLight; // Reference to the Light2D component

    // Intensity fluctuation range
    public float minIntensity = 0.5f;
    public float maxIntensity = 2.0f;

    // Flicker speed
    public float flickerSpeed = 10f;

    void Start()
    {
        // Get the Light2D component attached to this GameObject
        torchLight = GetComponent<Light2D>();
    }

    // Single Update method
    void Update()
    {
        if (torchLight != null)
        {
            // Apply exaggerated flickering using Mathf.PerlinNoise for randomness
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);
            torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }

        // Add any other existing logic from your original Update() here if needed
    }
}
