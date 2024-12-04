using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; 
    private int score = 0; 

   
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
}
