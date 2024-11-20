using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NavigationSystem;

public class LoadingBar : MonoBehaviour
{
    public Slider loadingBar;
    private float loadingValue = 0;

    // Update is called once per frame
    void Update()
    {
        int randomNum = Random.Range(0, 100);

        if(randomNum < 20 && loadingValue < 0.8f)
        {
            FakeLoading();
        }
        else if (loadingValue >= 0.8f)
        {
            StartCoroutine(completeLoad());
        }
    }

    public void FakeLoading()
    {
        loadingValue += 0.01f;
        loadingBar.value = loadingValue;
    }

    IEnumerator completeLoad()
    {
        yield return new WaitForSeconds(1);
        loadingValue = 1;
        loadingBar.value = loadingValue;
        NavigationManager.instance.LoadSceneRequest("Lobby");
    }
}
