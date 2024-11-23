using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject prefab;         // El prefab que se va a espawnear
    public Transform spawnPoint;      // Punto donde se espawnean los prefabs
    public float spawnInterval = 2f;  // Intervalo de tiempo entre spawns

    [Header("Random Spawn Settings")]
    public float minZ = -5f;          // Mínima posición Z para el spawn
    public float maxZ = 5f;           // Máxima posición Z para el spawn

    [Header("Movement Settings")]
    public float moveSpeed = 10f;      // Velocidad de movimiento de los prefabs
    public float moveLimit = 90f;     // Distancia máxima que puede recorrer el prefab

    private void Start()
    {
        // Inicia el proceso de espawnear prefabs repetidamente
        InvokeRepeating(nameof(SpawnPrefab), 0f, spawnInterval);
    }

    private void SpawnPrefab()
    {
        // Instancia el prefab en el punto de spawn con su rotación original
        GameObject newPrefab = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        // Agrega un script de movimiento al prefab instanciado
        newPrefab.AddComponent<MoveForward>().Initialize(moveSpeed, moveLimit);

        float randomZ = UnityEngine.Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, randomZ);
        //Vector3 randomPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, UnityEngine.Random.Range(minZ, maxZ));
        //Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(minZ, maxZ), spawnPoint.position.y, spawnPoint.position.z);
        //Vector3 randomPosition = new Vector3(spawnPoint.position.x, UnityEngine.Random.Range(minZ, maxZ), spawnPoint.position.z);
    }
}
