using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FundasScreenButtons : MonoBehaviour
{
    public Button returnShopButton;

    private void Awake()
    {
        returnShopButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("CasesCanvas"));
        returnShopButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("ShopCanvas"));
    }
}
