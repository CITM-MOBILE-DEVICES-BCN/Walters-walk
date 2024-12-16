using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NavigationSystem;

//public class NavigationSystemTest
//{
//    // Test the navigation system by loading different scenes and screens
//    [UnityTest]
//    public IEnumerator TestNavigationSystem()
//    {
//        // Load the splash screen using the SceneManager,
//        // needed to instantiate the GameManager with the NavigationSystem
//        SceneManager.LoadScene("SplashScreen");
//        yield return new WaitForSeconds(1);
//        // Navigate through the different scenes and screens using the NavigationSystem
//        GameManager.instance.LoadSceneRequest("Lobby");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.LoadSceneRequest("Meta");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.LoadScreenRequest("ShopCanvas");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.LoadScreenRequest("Buffs");
//        GameManager.instance.DestroyScreenRequest("ShopCanvas");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.LoadScreenRequest("ShopCanvas");
//        GameManager.instance.DestroyScreenRequest("Buffs");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.DestroyScreenRequest("ShopCanvas");
//        yield return new WaitForSeconds(1);
//        GameManager.instance.LoadSceneRequest("Game");
//        yield return new WaitForSeconds(1);

//        Assert.Pass();

//    }

//}
