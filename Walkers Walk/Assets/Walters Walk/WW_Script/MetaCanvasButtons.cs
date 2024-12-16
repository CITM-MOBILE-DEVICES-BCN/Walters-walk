using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class MetaCanvasButtons : MonoBehaviour
{
    public Button shopButton;
    public Button playButton;
    public Button returnLobbyButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        shopButton.onClick.AddListener(() => WW_GameManager.instance.LoadScreenRequest("ShopCanvas"));
        playButton.onClick.AddListener(() => WW_GameManager.instance.LoadSceneRequest("Game"));
        returnLobbyButton.onClick.AddListener(() => WW_GameManager.instance.LoadSceneRequest("Lobby"));
        currencyText.text = WW_GameManager.instance.playerData.GetCurrency();
    }

}
