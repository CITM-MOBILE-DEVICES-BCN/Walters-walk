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
        returnShopButton.onClick.AddListener(() => WW_GameManager.instance.DestroyScreenRequest("BuffsCanvas"));
        returnShopButton.onClick.AddListener(() => WW_GameManager.instance.LoadScreenRequest("ShopCanvas"));
        currencyText.text = WW_GameManager.instance.playerData.GetCurrency();
    }
}
