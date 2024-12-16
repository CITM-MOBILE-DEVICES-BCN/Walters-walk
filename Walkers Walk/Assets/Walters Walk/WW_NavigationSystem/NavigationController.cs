using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NavigationSystem
{
    public class NavigationController : MonoBehaviour
    {
        public List<string> sceneNames;
        public List<GameObject> screenPrefabs;
        public List<GameObject> activeScreens;
        public void LoadScene(string sceneName)
        {
            int index = sceneNames.IndexOf(sceneName);
            if (index != -1)
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void LoadScreen(string screenName, Transform parent)
        {
            var screenToInstantiate = screenPrefabs.Find(screen => screen.name == screenName);
            if (screenToInstantiate != null)
            {
                var screen = Instantiate(screenToInstantiate);
                screen.transform.SetParent(parent, false);
                this.activeScreens.Add(screen);
            }
        }

        public void DestroyScreen(string screenName)
        {
            screenName = screenName + "(Clone)";
            if (activeScreens.Find(screen => screen.name.StartsWith(screenName)) != null)
            {
                Destroy(activeScreens.Find(screen => screen.name == screenName));
            }
            activeScreens.Remove(activeScreens.Find(screen => screen.name == screenName));
        }

    }
}

