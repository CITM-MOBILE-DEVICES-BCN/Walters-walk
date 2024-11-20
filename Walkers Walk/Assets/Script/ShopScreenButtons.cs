using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreenButtons : MonoBehaviour
{
    public Button returnMenuButton;
    public Button buffsButton;
    public Button fundasButton;

    private void Awake()
    {
        returnMenuButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
        buffsButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("Buffs"));
        buffsButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
        fundasButton.onClick.AddListener(() => GameManager.instance.LoadScreenRequest("Fundas"));
        fundasButton.onClick.AddListener(() => GameManager.instance.DestroyScreenRequest("ShopCanvas"));
    }
}
