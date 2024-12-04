using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class ShopScreenButtons : MonoBehaviour
{
    public Button returnMenuButton;
    public Button buffsButton;
    public Button fundasButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnMenuButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
        buffsButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("BuffsCanvas"));
        buffsButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
        fundasButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("CasesCanvas"));
        fundasButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
        currencyText.text = GameManager.instance.playerData.GetCurrency();
    }
}
