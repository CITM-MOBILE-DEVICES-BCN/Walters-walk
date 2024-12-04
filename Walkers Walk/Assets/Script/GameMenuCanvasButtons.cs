using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;


public class GameMenuCanvasButtons : MonoBehaviour
{
    public Button returnMetaButton;
    public TextMeshProUGUI currencyText;

    private void Awake()
    {
        returnMetaButton.onClick.AddListener(() => ReturnMeta());
        currencyText.text = GameManager.instance.playerData.GetCurrency();
    }

    private void ReturnMeta()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        scoreManager.ResetScore();
        GameManager.instance.LoadSceneRequest("Meta");
    }
}
