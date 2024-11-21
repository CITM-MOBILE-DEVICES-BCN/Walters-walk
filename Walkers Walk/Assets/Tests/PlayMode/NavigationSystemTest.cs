using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NavigationSystem;

public class NavigationSystemTest
{

    [UnityTest]
    public IEnumerator TestNavigationSystem()
    {
        SceneManager.LoadScene("SplashScreen");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadSceneRequest("Lobby");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadSceneRequest("Meta");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadScreenRequest("ShopCanvas");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadScreenRequest("Buffs");
        GameManager.instance.DestroyScreenRequest("ShopCanvas");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadScreenRequest("ShopCanvas");
        GameManager.instance.DestroyScreenRequest("Buffs");
        yield return new WaitForSeconds(1);
        GameManager.instance.DestroyScreenRequest("ShopCanvas");
        yield return new WaitForSeconds(1);
        GameManager.instance.LoadSceneRequest("Game");
        yield return new WaitForSeconds(1);

        Assert.Pass();

    }

}
