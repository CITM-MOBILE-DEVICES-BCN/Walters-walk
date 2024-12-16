using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NavigationSystem;
using System;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    private NavigationManager navManager;
    private SaveSystem saveSystem;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        saveSystem = new SaveSystem();
       
    }
    #endregion

    public Vector2 checkpointPosition;
   
    private void Start()
    {

        navManager = new NavigationManager();
        SaveData saveData = saveSystem.Load();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            saveSystem.DeleteSave();
        }
    }

    public void UpdateCheckpointPosition(Vector2 pos)
    {
        checkpointPosition = pos;

        SaveData saveData = new SaveData()
        {
            lastCheckpointPosition = checkpointPosition
        };

        saveSystem.Save(saveData);
    }

    public Vector2 GetCheckpointPosition()
    {
        return checkpointPosition;
    }

   
    public void ChangeScene(string sceneName)
    {
        navManager.ChangeScene(sceneName);
        

        ScoreManager.Instance.ResetLevelScore();
    }
    public void GoBackToLevelSelector(string sceneName)
    {
        navManager.ChangeScene(sceneName);
        
        if (MetaUIManager.Instance != null)
        {
            MetaUIManager.Instance.ShowLevelSelectScreen();
        }
        else
        {
            Debug.LogWarning("MetaUIManager Instance not found!");
        }
    }

    public void ActivateCanvas(GameObject canvas)
    {
        if (navManager == null)
        {
            navManager = new NavigationManager();
        }

        navManager.ActivateCanvas(canvas);
        
    }

    public void DeactivateCanvas(GameObject canvas)
    {
        navManager.DeactivateCanvas(canvas);
        
    }
    public void FinishLevel()
    {
        if (ScoreManager.Instance != null)
        {
            SoundManager.PlaySound(SoundType.LEVELCOMPLETED);
            ScoreManager.Instance.FinishLevel();
            Debug.Log("Level finished through GameManager");
        }
        else
        {
            Debug.LogError("ScoreManager instance is null when trying to finish level");
        }
    }

    public void WaitForSeconds(float delay)
    {
        StartCoroutine(WaitCoroutine(delay));
    }

    private IEnumerator WaitCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
}


