using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    public Slider slider;
    public float smoothSpeed = 0.2f;
    public string nextSceneName = "NextScene";

    private float targetProgress = 0f;

    void Start()
    {
        slider.value = 0f;
        InvokeRepeating("TestLoad", 0f, 0.5f);
    }

    void Update()
    {
        slider.value = Mathf.Lerp(slider.value, targetProgress, smoothSpeed * Time.deltaTime);

        if (slider.value >= 0.99f)
        {
            slider.value = 1f;
            GameManager.Instance.ChangeScene(nextSceneName);
        }

    }

    public void IncreaseProgress(float newProgress)
    {
        targetProgress = Mathf.Clamp(newProgress, 0f, 1f);
    }

    void TestLoad()
    {
        IncreaseProgress(slider.value + 0.1f);
    }

}
