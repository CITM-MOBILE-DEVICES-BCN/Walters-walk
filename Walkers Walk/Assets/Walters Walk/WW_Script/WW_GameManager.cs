using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WW_NavigationSystem;


public class WW_GameManager : MonoBehaviour
{
    public static WW_GameManager instance;
    public GameObject canvasForInstantiations;
    public NavigationController navigationController;
    public PlayerData playerData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        playerData.Load();
    }

    public void LoadSceneRequest(string sceneName)
    {
        navigationController.LoadScene(sceneName);
    }

    public void LoadScreenRequest(string screenName)
    {
        navigationController.LoadScreen(screenName, canvasForInstantiations.transform);
    }

    public void DestroyScreenRequest(string screenName)
    {
        navigationController.DestroyScreen(screenName);
    }


}


