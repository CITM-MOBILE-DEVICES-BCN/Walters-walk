using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMenus : MonoBehaviour
{
    public GameObject buffsShop, casesShop;
    public bool buffON;
    public bool caseON;
    
    void Start()
    {
        buffsShop.SetActive(false); casesShop.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
