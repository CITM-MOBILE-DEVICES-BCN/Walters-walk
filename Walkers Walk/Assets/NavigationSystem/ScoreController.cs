using TMPro;
using UnityEngine;


namespace ScoreSystem
{
    public class ScoreController : MonoBehaviour
    {
        public int score;
        public GameObject scoreText;

        private void Start()
        {
            score = 0;
            UpdateScoreText();
        }
        public void AddScore(int scoreToAdd)
        {
            score += scoreToAdd;
            UpdateScoreText();
        }

        public void SubtractScore(int scoreToSubtract)
        {
            score -= scoreToSubtract;
            UpdateScoreText();
        }

        public void UpdateScoreText()
        {
            scoreText = GameObject.Find("CurrencyValue");
            if (scoreText != null)
            {
                Debug.Log("Object found: " + scoreText.name);
                scoreText.GetComponent<TextMeshProUGUI>().text = score.ToString();
            }
            else Debug.Log("Object not found!");
        }
    }
}