using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    public event Action<int> OnGemsChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnStarsChanged;
    public event Action<int> OnTotalGemsChanged;
    public event Action OnProgressUpdated;


    #region Singleton
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        saveSystem = new SaveSystem();
        LoadProgress();
    }
    #endregion

    [Header("Score Settings")]
    [SerializeField] private int gemsPerStar = 5;
    [SerializeField] private int pointsPerStar = 10000;

    private SaveSystem saveSystem;
    private SaveData saveData;
    private int currentGems;
    private int totalScore;
    private int gemsAtLastCheckpoint;
    private bool hasLevelBeenCompleted = false;


    public int CurrentGems => currentGems;
    public int TotalScore => saveData?.totalScore ?? 0;
    public int TotalGems => saveData?.totalGems ?? 0;
    public bool IsLevelCompleted => hasLevelBeenCompleted;

    private void LoadProgress()
    {
        if (saveSystem == null)
        {
            saveSystem = new SaveSystem();
        }

        saveData = saveSystem.Load();
        if (saveData == null)
        {
            saveData = new SaveData
            {
                levelProgress = new System.Collections.Generic.List<LevelData>(),
                totalScore = 0,
                totalGems = 0
            };
        }

        Debug.Log("Progress Loaded - Total Score: " + saveData.totalScore);
    }
    public (int stars, int maxStars) GetLevelProgress(string levelName)
    {
        if (saveData?.levelProgress == null) return (0, 3);

        var levelData = saveData.levelProgress.FirstOrDefault(l => l.levelName == levelName);
        return (levelData?.stars ?? 0, 3);
    }
    public void AddGems(int amount)
    {
        currentGems += amount;
        OnGemsChanged?.Invoke(currentGems);
    }

    public bool TrySpendGems(int amount)
    {
        if (currentGems >= amount)
        {
            currentGems -= amount;
            OnGemsChanged?.Invoke(currentGems);
            return true;
        }
        return false;
    }

    public void SaveCheckpoint()
    {
        gemsAtLastCheckpoint = currentGems;
    }

    public void ResetToLastCheckpoint()
    {
        currentGems = gemsAtLastCheckpoint;
        OnGemsChanged?.Invoke(currentGems);
    }

    public int CalculateStars()
    {
        return Mathf.Min(3, currentGems / gemsPerStar);

    }

    public void FinishLevel()
    {
        if (hasLevelBeenCompleted) return;

        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int stars = CalculateStars();

        // Update level progress
        var levelData = saveData.levelProgress.FirstOrDefault(l => l.levelName == currentLevel);
        if (levelData == null)
        {
            levelData = new LevelData { levelName = currentLevel };
            saveData.levelProgress.Add(levelData);
        }

        // Only update if we got more stars
        if (stars > levelData.stars)
        {
            int starDifference = stars - levelData.stars;
            saveData.totalScore += starDifference * pointsPerStar;
            levelData.stars = stars;
        }

        // Update gems if we collected more
        if (currentGems > levelData.highestGems)
        {
            int gemDifference = currentGems - levelData.highestGems;
            saveData.totalGems += gemDifference;
            levelData.highestGems = currentGems;
        }

        OnScoreChanged?.Invoke(totalScore);
        OnStarsChanged?.Invoke(stars);
        OnTotalGemsChanged?.Invoke(saveData.totalGems);
        OnProgressUpdated?.Invoke();

        hasLevelBeenCompleted = true;

        saveSystem.Save(saveData);

        Debug.Log("Level has finished");
        Debug.Log($"Stars: {stars}");
        Debug.Log($"Score: {totalScore}");
        Debug.Log($"Gems: {currentGems}");
        Debug.Log($"Total Gems: {saveData.totalGems}");
    }

    public void ResetLevelScore()
    {
        currentGems = 0;
        gemsAtLastCheckpoint = 0;
        hasLevelBeenCompleted = false;
        OnGemsChanged?.Invoke(currentGems);
    }
}
