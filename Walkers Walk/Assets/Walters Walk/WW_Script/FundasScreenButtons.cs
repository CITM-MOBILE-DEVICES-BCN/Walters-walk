using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using WW_NavigationSystem;

public class FundasScreenButtons : MonoBehaviour
{
    public Button returnShopButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnShopButton.onClick.AddListener(() => NavigationController.instance.DestroyScreen("CasesCanvas"));
        returnShopButton.onClick.AddListener(() => NavigationController.instance.LoadScreen("ShopCanvas",null));
        currencyText.text = WW_GameManager.instance.playerData.GetCurrency();
    }
}
