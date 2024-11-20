using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetaCanvasButtons : MonoBehaviour
{
    public Button shopButton;
    public Button playButton;
    public Button returnLobbyButton;

    private void Awake()
    {
        shopButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("ShopCanvas"));
        playButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Game"));
        returnLobbyButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Lobby"));
    }

}
