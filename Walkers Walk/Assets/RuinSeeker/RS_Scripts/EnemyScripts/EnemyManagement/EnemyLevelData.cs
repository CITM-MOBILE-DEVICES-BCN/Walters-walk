using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLevelData", menuName = "Level/EnemyData")]
public class EnemyLevelData : ScriptableObject
{
    public List<EnemySpawnPoint> enemySpawnPoints;
}
