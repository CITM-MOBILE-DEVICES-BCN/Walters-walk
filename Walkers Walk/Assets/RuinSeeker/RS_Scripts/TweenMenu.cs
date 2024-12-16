using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenMenu : MonoBehaviour
{
    public GameObject maniMenu;
    public GameObject levelMenu;

    public Transform[] positions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("ActivateLevelMenu")]
    public void ActivateLevelMenu()
    {
        levelMenu.transform.position = positions[0].position;

        levelMenu.transform.DOMoveY(290, 0.4f, true);

        maniMenu.transform.DOMoveY(positions[1].position.y, 0.4f, true);
    }

    [ContextMenu("DeactivateLevelMenu")]
    public void DeactivateLevelMenu()
    {

        maniMenu.transform.position = positions[0].position;
        maniMenu.transform.DOMoveY(290, 0.4f, true);
        levelMenu.transform.DOMoveY(positions[1].position.y, 0.4f, true);
    }

}
