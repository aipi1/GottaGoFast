using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages spawning of obstacles
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    private Vector3 spawnPosition = new Vector3(25, 0, 0);
    private float startDelay = 2.0f;
    private float repeatRate = 2.0f;

    void OnEnable()
    {
        InvokeRepeating("SpawnRandomObstacle", startDelay, repeatRate);
    }

    void OnDisable()
    {
        CancelInvoke("SpawnRandomObstacle");
    }

    public void SpawnRandomObstacle()
    {
        int maxIndex;
        int minIndex;
        if (GameManager.cityLevel)
        {
            minIndex = 0;
            maxIndex = 2;
        }
        else
        {
            minIndex = 2;
            maxIndex = obstaclePrefabs.Length;
        }
        var index = Random.Range(minIndex, maxIndex);
        var obstaclePrefab = obstaclePrefabs[index];

        Instantiate(obstaclePrefab, spawnPosition, obstaclePrefab.transform.rotation);
    }
}
