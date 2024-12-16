using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class BuyPowerUp : MonoBehaviour
{
    public int powerUpCost;
    public Button buyPowerUpButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        buyPowerUpButton.onClick.AddListener(() => BuyPowerUpAction());
    }

    private void BuyPowerUpAction()
    {
        if (GameManager.instance.playerData.CanAfford(powerUpCost))
        {
            GameManager.instance.playerData.DecreaseCurrency(powerUpCost);
            GameManager.instance.playerData.Save();
            currencyText.text = GameManager.instance.playerData.GetCurrency();
            Debug.Log("Power up bought");
        }
        else
        {
            Debug.Log("Not enough currency");
        }
    }
}
