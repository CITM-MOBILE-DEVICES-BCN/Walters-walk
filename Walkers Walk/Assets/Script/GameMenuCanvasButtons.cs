using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameMenuCanvasButtons : MonoBehaviour
{
    public Button returnMetaButton;

    private void Awake()
    {
        returnMetaButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Meta"));
    }
}
