using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffsScreenButtons : MonoBehaviour
{
    public Button returnShopButton;
    
    private void Awake()
    {
        returnShopButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("BuffsCanvas"));
        returnShopButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("ShopCanvas"));
    }
}
