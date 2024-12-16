using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject[] prefabs;
    public Transform spawnPoint;
    public float spawnInterval = 2f;

    [Header("Random Spawn Settings")]
    [Range(0f, 10f)]
    public float perpendicularOffset = 5f;
    [Range(0f, 10f)]
    public float alignedOffset = 5f;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float moveLimit = 90f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnPrefab), 0f, spawnInterval);
    }

    private void SpawnPrefab()
    {
        Vector3 pOffset = transform.right * Random.Range(-perpendicularOffset, perpendicularOffset);
        Vector3 aOffset = transform.forward * Random.Range(-alignedOffset, alignedOffset);

        GameObject newPrefab = Instantiate(
            prefabs[Random.Range(0, prefabs.Length)],
            spawnPoint.position + pOffset + aOffset,
            spawnPoint.rotation
        );
        newPrefab.AddComponent<MoveForward>().Initialize(moveSpeed, moveLimit);
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Vector3 areaCenter = spawnPoint.position;

            Vector3 areaSize = transform.right * perpendicularOffset * 2 + transform.forward * alignedOffset * 2;

            Gizmos.DrawCube(areaCenter, areaSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}
