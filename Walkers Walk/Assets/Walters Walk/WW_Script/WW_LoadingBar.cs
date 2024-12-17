using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WW_NavigationSystem;

public class WW_LoadingBar : MonoBehaviour
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
        loadingValue += 0.04f;
        loadingBar.value = loadingValue;
    }

    IEnumerator completeLoad()
    {
        yield return new WaitForSeconds(.2f);
        loadingValue = 1;
        loadingBar.value = loadingValue;
        NavigationController.instance.LoadScene("Lobby");
    }
}
