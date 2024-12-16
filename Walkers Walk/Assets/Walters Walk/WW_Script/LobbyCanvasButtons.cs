using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvasButtons : MonoBehaviour
{
    public Button WalterWalkButton;

    private void Awake()
    {
        WalterWalkButton.onClick.AddListener(() => GameManager.instance.LoadSceneRequest("Meta"));
    }
}
