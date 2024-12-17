using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using WW_NavigationSystem;

public class MetaCanvasButtons : MonoBehaviour
{
    public Button shopButton;
    public Button playButton;
    public Button returnLobbyButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        shopButton.onClick.AddListener(() => NavigationController.instance.LoadScreen("ShopCanvas",null));
        playButton.onClick.AddListener(() => NavigationController.instance.LoadScene("Game"));
        returnLobbyButton.onClick.AddListener(() => ReturnToLobby());
        currencyText.text = WW_GameManager.instance.playerData.GetCurrency();
    }
    
    public void ReturnToLobby()
    {
        //Destroy GameManager
        NavigationController.instance.LoadScene("Lobby");
    }
}
