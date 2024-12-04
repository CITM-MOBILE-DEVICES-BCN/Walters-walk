using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class BuffsScreenButtons : MonoBehaviour
{
    public Button returnShopButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnShopButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("BuffsCanvas"));
        returnShopButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("ShopCanvas"));
        currencyText.text = GameManager.instance.playerData.GetCurrency();
    }
}
