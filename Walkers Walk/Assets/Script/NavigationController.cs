using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public void LoadSceneRequest(string sceneName)
    {
        GameManager.instance.LoadScene(sceneName);
    }

    public void LoadScreenRequest(string screenName)
    {
        GameManager.instance.LoadScreen(screenName);
    }

    public void DestroyScreenRequest(string screenName)
    {
       GameManager.instance.DestroyScreen(screenName);
    }
    
}
