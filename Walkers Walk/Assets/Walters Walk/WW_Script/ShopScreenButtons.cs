using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using WW_NavigationSystem;

public class ShopScreenButtons : MonoBehaviour
{
    public Button returnMenuButton;
    public Button buffsButton;
    public Button fundasButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnMenuButton.onClick.AddListener(() => NavigationController.instance.DestroyScreen("ShopCanvas"));
        buffsButton.onClick.AddListener(() => NavigationController.instance.LoadScreen("BuffsCanvas", null));
        buffsButton.onClick.AddListener(() => NavigationController.instance.DestroyScreen("ShopCanvas"));
        fundasButton.onClick.AddListener(() => NavigationController.instance.LoadScreen("CasesCanvas", null));
        fundasButton.onClick.AddListener(() => NavigationController.instance.DestroyScreen("ShopCanvas"));
        currencyText.text = WW_GameManager.instance.playerData.GetCurrency();
    }
}
