using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class FundasScreenButtons : MonoBehaviour
{
    public Button returnShopButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnShopButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("CasesCanvas"));
        returnShopButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("ShopCanvas"));
        currencyText.text = GameManager.instance.playerData.GetCurrency();
    }
}
