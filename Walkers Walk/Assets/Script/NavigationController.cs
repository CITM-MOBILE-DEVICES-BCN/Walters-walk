using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    public List<string> sceneNames;
    public List<GameObject> screenPrefabs;
    public List<GameObject> activeScreens;
    public static NavigationController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void LoadScene(string sceneName)
    {
        int index = sceneNames.IndexOf(sceneName);
        if (index != -1)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void LoadScreen(string screenName)
    {
        var screenToInstantiate = screenPrefabs.Find(screen => screen.name == screenName);
        if (screenToInstantiate != null)
        {
            var screen = Instantiate(screenToInstantiate);
            activeScreens.Add(screen);
        }
    }

    public void DestroyScreen(string screenName)
    {
       if (activeScreens.Find(screen => screen.name == screenName) != null)
       {
           Destroy(activeScreens.Find(screen => screen.name == screenName));
       }
       activeScreens.Remove(activeScreens.Find(screen => screen.name == screenName));
    }
    
}
