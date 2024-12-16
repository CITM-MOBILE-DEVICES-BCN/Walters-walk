using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NavigationSystem
{
    public class NavigationManager
    {
        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void ActivateCanvas(GameObject canvas)
        {
            if (canvas != null)
            {
                canvas.SetActive(true);
            }
        }

        public void DeactivateCanvas(GameObject canvas)
        {
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }
    }
}
