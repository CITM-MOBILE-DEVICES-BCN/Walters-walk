using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    public TextMeshProUGUI scoreText; 
    private int score = 0;

    private void OnEnable()
    {
        playerData.OnDeathAction += ResetScore;
    }

    private void OnDisable()
    {
        playerData.OnDeathAction -= ResetScore;
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }
  
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void ResetScore()
    {
        if(score > 0)
        {
            playerData.IncreaseCurrency(score);
            playerData.Save();
        }
        score = 0;
        UpdateScoreText();
    }
}
