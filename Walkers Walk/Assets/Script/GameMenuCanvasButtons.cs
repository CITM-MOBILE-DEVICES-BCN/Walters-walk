using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuCanvasButtons : MonoBehaviour
{
    public Button returnMetaButton;
    private void Awake()
    {
        returnMetaButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Meta"));
    }
}
