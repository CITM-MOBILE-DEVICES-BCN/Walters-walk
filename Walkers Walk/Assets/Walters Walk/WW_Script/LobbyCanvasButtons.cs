using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WW_NavigationSystem;

public class LobbyCanvasButtons : MonoBehaviour
{
    public Button WalterWalkButton;
    public Button RuinSeeker;

    private void Awake()
    {
        WalterWalkButton.onClick.AddListener(() => NavigationController.instance.LoadScene("Meta"));
        RuinSeeker.onClick.AddListener(() => NavigationController.instance.LoadScene("3.Meta"));
    }
}
