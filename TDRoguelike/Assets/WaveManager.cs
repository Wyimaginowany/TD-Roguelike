﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] Wave[] waves;
    [SerializeField] Transform spawnPoint;

    int currentWaveIndex = 0;
    int currentMiniWaveIndex = 0;

    [System.Serializable]
    public class Wave
    {
        public MiniWave[] miniWaves;
        public int goldForWaveCompleated = 10;
    }

    [System.Serializable]
    public class MiniWave
    {
        public GameObject enemyPrefab;
        public int amount;
        public float spawnRate;
        public float timeAfterWave;
    }

    private void Start()
    {
        StartCoroutine(spawnWave(1f));
    }

    private IEnumerator spawnWave(float timeBeforeSpawn)
    {
        if (currentMiniWaveIndex < waves.Length)
        { 
            yield return new WaitForSeconds(timeBeforeSpawn);
            MiniWave currentMiniWave = waves[currentWaveIndex].miniWaves[currentMiniWaveIndex];
            StartCoroutine(spawnMiniWave(currentMiniWave.spawnRate, currentMiniWave));
            currentMiniWaveIndex++;
        }
        else
        {
            Debug.Log("end of big wave");
            currentWaveIndex++;
        }
    }

    private IEnumerator spawnMiniWave(float spawnRate, MiniWave miniWave)
    {
        for (int i = 0; i < miniWave.amount; i++)
        {
            SpawnEnemy(miniWave.enemyPrefab);
            yield return new WaitForSeconds(1f / miniWave.spawnRate);
        }
        Debug.Log("end of miniWave");
        StartCoroutine(spawnWave(miniWave.timeAfterWave));
    }

    private void SpawnEnemy(GameObject enemy)
    {
        Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        Debug.Log(enemy.name);
    }
}