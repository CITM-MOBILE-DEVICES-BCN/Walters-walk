using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternatingRoadSpawner : MonoBehaviour
{
    public Transform player; // El jugador (o la cámara en primera persona)
    public GameObject roadPrefab1; // Primer prefab de carretera
    public GameObject roadPrefab2; // Segundo prefab de carretera
    public float spawnDistance = 20f; // Distancia para spawnear la carretera delante del jugador
    public float deleteDistance = 10f; // Distancia para eliminar las carreteras detrás del jugador
    public float spacing = 50f; // Distancia entre cada carretera generada

    private Queue<GameObject> spawnedRoads = new Queue<GameObject>(); // Cola para manejar las carreteras generadas
    private float lastSpawnZ = 0f; // Última posición Z donde se generó una carretera

    void Start()
    {
        // Inicializa las primeras carreteras
        lastSpawnZ = player.position.z;
        SpawnInitialRoads();
    }

    void Update()
    {
        // Comprobar si el jugador ha avanzado lo suficiente para generar una nueva carretera
        if (player.position.z > lastSpawnZ - spawnDistance)
        {
            SpawnRoad();
        }

        // Eliminar las carreteras que ya ha pasado el jugador
        DeleteOldRoads();
    }

    // Genera un conjunto inicial de carreteras
    private void SpawnInitialRoads()
    {
        for (float z = player.position.z; z < player.position.z + spawnDistance; z += spacing)
        {
            Vector3 spawnPosition = new Vector3(0, 0, z);
            GameObject road = Instantiate(SelectRandomRoadPrefab(), spawnPosition, Quaternion.identity);
            spawnedRoads.Enqueue(road); // Agregar la carretera a la cola
        }
    }

    // Genera una nueva carretera delante del jugador, alternando entre dos prefabs
    private void SpawnRoad()
    {
        Vector3 spawnPosition = new Vector3(0, 0, lastSpawnZ + spacing);
        GameObject newRoad = Instantiate(SelectRandomRoadPrefab(), spawnPosition, Quaternion.identity);
        spawnedRoads.Enqueue(newRoad);
        lastSpawnZ += spacing; // Actualiza la última posición Z donde se generó la carretera
    }

    // Selecciona aleatoriamente entre dos prefabs de carretera
    private GameObject SelectRandomRoadPrefab()
    {
        return Random.value > 0.5f ? roadPrefab1 : roadPrefab2;
    }

    // Elimina las carreteras que están demasiado atrás del jugador
    private void DeleteOldRoads()
    {
        if (spawnedRoads.Count > 0)
        {
            GameObject firstRoad = spawnedRoads.Peek(); // Obtener la primera carretera de la cola
            if (firstRoad.transform.position.z < player.position.z - deleteDistance)
            {
                Destroy(firstRoad); // Eliminar la carretera
                spawnedRoads.Dequeue(); // Eliminarla de la cola
            }
        }
    }
}