using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// This class will manage all the UI elements in the gameplay scene
public class ScoreUIManager : MonoBehaviour
{
    #region Singleton
    public static ScoreUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform starContainer;
    [SerializeField] private Image[] starImages;

    private void Start()
    {
        // Subscribe to score events
        ScoreManager.Instance.OnGemsChanged += UpdateGemsDisplay;
        ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        ScoreManager.Instance.OnStarsChanged += UpdateStarsDisplay;

        // Initialize displays
        UpdateGemsDisplay(ScoreManager.Instance.CurrentGems);
        UpdateScoreDisplay(ScoreManager.Instance.TotalScore);
        UpdateStarsDisplay(ScoreManager.Instance.CalculateStars());
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnGemsChanged -= UpdateGemsDisplay;
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            ScoreManager.Instance.OnStarsChanged -= UpdateStarsDisplay;
        }
    }

    public void UpdateGemsDisplay(int gems)
    {
        if (gemsText != null)
        {
            gemsText.text = $"Gems: {gems}";
        }
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateStarsDisplay(int stars)
    {
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].enabled = i < stars;
            }
        }
    }
}