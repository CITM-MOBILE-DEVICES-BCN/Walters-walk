using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyBuilder
{
    public static T BuildEnemy<T>(Vector3 postion, string enemyType) where T : Enemy
    {
        GameObject enemyPrefab = Resources.Load<GameObject>("EnemyPrefabs/" + enemyType);
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not found for " + typeof(T).Name);
            return null;
        }

        GameObject enemy = GameObject.Instantiate(enemyPrefab, postion, Quaternion.identity);
        return enemy.GetComponent<T>();
    }
}
