using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameMenuCanvasButtons : MonoBehaviour
{
    public Button returnMetaButton;

    public GameObject targetObject; // El GameObject que queremos activar/desactivar

    public Button targetButton; // El botón que será desactivado/activado
    private bool isTargetButtonActive = true; // Estado actual del botón

    public UnityEngine.UI.Image targetImage; // La imagen a reducir
    public float shrinkDuration = 5f; // Tiempo total en segundos para reducir a 0

    private float initialHeight; // Altura inicial de la imagen
    private float elapsedTime = 0f; // Tiempo transcurrido

    private void Start()
    {
        if (targetImage != null)
        {
            // Guardar la altura inicial de la imagen
            initialHeight = targetImage.rectTransform.sizeDelta.y;
        }
    }
    private void Awake()
    {
        returnMetaButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Meta"));
    }

    void Update()
    {
        if (targetImage != null && elapsedTime < shrinkDuration)
        {
            // Incrementar el tiempo transcurrido
            elapsedTime += Time.deltaTime;

            // Calcular la nueva altura basada en el tiempo transcurrido
            float newHeight = Mathf.Lerp(initialHeight, 0f, elapsedTime / shrinkDuration);

            // Aplicar la nueva altura a la imagen
            Vector2 newSize = targetImage.rectTransform.sizeDelta;
            newSize.y = newHeight;
            targetImage.rectTransform.sizeDelta = newSize;
        }
    }
    

    public void ToggleActiveState()
    {
        if (targetObject != null)
        {
            // Cambia el estado de activo/inactivo
            targetObject.SetActive(!targetObject.activeSelf);
        }
        
    }

    public void ToggleButton()
    {
        if (targetButton != null)
        {
            isTargetButtonActive = !isTargetButtonActive; // Cambiar el estado
            targetButton.interactable = isTargetButtonActive; // Activar o desactivar el botón
        }
    }
}
