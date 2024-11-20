using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NavigationSystem
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager instance;
        public NavigationController navigationController;

        public GameObject canvasForInstantiations;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        public void LoadSceneRequest(string sceneName)
        {
            navigationController.LoadScene(sceneName);
        }

        public void LoadScreenRequest(string screenName)
        {
            navigationController.LoadScreen(screenName);
        }

        public void DestroyScreenRequest(string screenName)
        {
            navigationController.DestroyScreen(screenName);
        }


    }
}

