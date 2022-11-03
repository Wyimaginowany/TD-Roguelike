﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Wave[] waves;
    [SerializeField] float timeBeforeFirstWave = 2f;

    [Header("To Attach")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject waveEndCanvas;
    [SerializeField] GameObject baseDestroyCanvas;

    //remove serialize
    [SerializeField] List<GameObject> aliveEnemies = new List<GameObject>();

    int currentWaveIndex = 0;
    int currentMiniWaveIndex = 0;
    bool waveCompleated = false;

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
        PlayerBase.OnBaseDestroyed += HandleBaseDestruction;
        EnemyHealth.OnEnemyDeath += HandleDeath;
    }

    public void SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            //to change later
            EnemyHealth.OnEnemyDeath -= HandleDeath;
            PlayerBase.OnBaseDestroyed -= HandleBaseDestruction;
            return;
        }

        baseDestroyCanvas.SetActive(false);
        waveEndCanvas.SetActive(false);
        waveCompleated = false;
        StartCoroutine(spawnWave(timeBeforeFirstWave));
    }

    private IEnumerator spawnWave(float timeBeforeSpawn)
    {
        if (currentMiniWaveIndex < waves[currentWaveIndex].miniWaves.Length)
        { 
            yield return new WaitForSeconds(timeBeforeSpawn);
            MiniWave currentMiniWave = waves[currentWaveIndex].miniWaves[currentMiniWaveIndex];
            StartCoroutine(spawnMiniWave(currentMiniWave.spawnRate, currentMiniWave));
            currentMiniWaveIndex++;
        }
        else
        {
            //Debug.Log("end of big wave");
            waveCompleated = true;
            currentMiniWaveIndex = 0;
            currentWaveIndex++;
        }
    }

    private IEnumerator spawnMiniWave(float spawnRate, MiniWave miniWave)
    {
        for (int i = 0; i < miniWave.amount; i++)
        {
            yield return new WaitForSeconds(1f / miniWave.spawnRate);
            SpawnEnemy(miniWave.enemyPrefab);
        }
        //Debug.Log("end of miniWave");
        StartCoroutine(spawnWave(miniWave.timeAfterWave));
    }

    private void SpawnEnemy(GameObject enemy)
    {
        GameObject newEnemy = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        aliveEnemies.Add(newEnemy);
        //Debug.Log(enemy.name);
    }

    private void HandleDeath(GameObject enemy)
    {
        if (waveCompleated && aliveEnemies.Count == 1)
        {
            //elast enemy arrived
            aliveEnemies.Remove(enemy);
            EndWave();
            return;
        }

        aliveEnemies.Remove(enemy);
        if (!waveCompleated) return;
        if (aliveEnemies.Count != 0) return;

        EndWave();
    }

    private void EndWave()
    {
        waveEndCanvas.SetActive(true);
        GameObject.FindGameObjectWithTag("TowerManager").GetComponent<TowerManager>()
              .GiveMoney(waves[currentWaveIndex - 1].goldForWaveCompleated);
    }

    private void HandleBaseDestruction()
    {
        StopAllCoroutines();
        foreach (GameObject enemy in aliveEnemies)
        {
            Destroy(enemy);
        }

        aliveEnemies.Clear();

        currentWaveIndex = 0;
        currentMiniWaveIndex = 0;
        waveCompleated = false;

        baseDestroyCanvas.SetActive(true);
    }

    public void ShowTowerShop()
    {
        baseDestroyCanvas.SetActive(false);
        waveEndCanvas.SetActive(true);
    }

}
