﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WaveScriptableObject[] _waves;
    [SerializeField] private float _timeBeforeFirstWave = 2f;

    [Header("To Attach")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private GameObject _enemyDirectionArrow;

    public static event Action<int, int, bool> OnWaveEnd;
    public static event Action<EnemyHealth> OnMiniWaveStart;

    private List<GameObject> _aliveEnemies = new List<GameObject>();
    private int _currentWaveIndex = 0;
    private int _currentMiniWaveIndex = 0;
    private bool _waveCompleated = false;

    //for simple dev tools
    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _currentMiniWaveIndex = 0;
            _currentWaveIndex++;
            EndWave();
        }
    }
    */

    private void Start()
    {
        PlayerHealth.OnPlayerDeath += HandleGameOver;
        PlayerBase.OnBaseDestroyed += HandleGameOver;
        EnemyHealth.OnEnemyDeath += HandleDeath;
        TowerManager.OnNextWaveButtonClicked += SpawnNextWave;
    }

    private void OnDestroy()
    {
        PlayerHealth.OnPlayerDeath -= HandleGameOver;
        PlayerBase.OnBaseDestroyed -= HandleGameOver;
        EnemyHealth.OnEnemyDeath -= HandleDeath;
        TowerManager.OnNextWaveButtonClicked -= SpawnNextWave;
    }

    public void SpawnNextWave()
    {
        if (_currentWaveIndex >= _waves.Length) return;
        if(_enemyDirectionArrow.activeSelf) _enemyDirectionArrow.SetActive(false);
            
        _waveCompleated = false;
        StartCoroutine(spawnWave(_timeBeforeFirstWave));
    }

    private IEnumerator spawnWave(float timeBeforeSpawn)
    {
        if (_currentMiniWaveIndex < _waves[_currentWaveIndex].MiniWaves.Length)
        { 
            yield return new WaitForSeconds(timeBeforeSpawn);
            MiniWave currentMiniWave = _waves[_currentWaveIndex].MiniWaves[_currentMiniWaveIndex];
            StartCoroutine(spawnMiniWave(currentMiniWave.SpawnRate, currentMiniWave));
            _currentMiniWaveIndex++;
        }
        else
        {
            //Debug.Log("end of big wave");
            _waveCompleated = true;
            _currentMiniWaveIndex = 0;
            _currentWaveIndex++;
        }
    }

    private IEnumerator spawnMiniWave(float spawnRate, MiniWave miniWave)
    {
        OnMiniWaveStart?.Invoke(miniWave.EnemyPrefab.GetComponent<EnemyHealth>());

        bool dontWaitForNextWave = miniWave.dontWaitWithNextWave;

        for (int i = 0; i < miniWave.Amount; i++)
        {
            yield return new WaitForSeconds(1f / miniWave.SpawnRate);
            SpawnEnemy(miniWave.EnemyPrefab);
            if (dontWaitForNextWave)
            {
                StartCoroutine(spawnWave(miniWave.TimeAfterWave));
                dontWaitForNextWave = false;
            }
        }
        //Debug.Log("end of miniWave");
        if (!miniWave.dontWaitWithNextWave) StartCoroutine(spawnWave(miniWave.TimeAfterWave));
    }

    private void SpawnEnemy(GameObject enemy)
    {
        GameObject newEnemy = Instantiate(enemy, _spawnPoint.position, Quaternion.identity);
        _aliveEnemies.Add(newEnemy);
    }

    private void HandleDeath(EnemyHealth enemy)
    {
        if (_waveCompleated && _aliveEnemies.Count == 1)
        {
            //last enemy arrived
            _aliveEnemies.Remove(enemy.gameObject);
            EndWave();
            return;
        }

        _aliveEnemies.Remove(enemy.gameObject);
        if (!_waveCompleated) return;
        if (_aliveEnemies.Count != 0) return;
        //last enemy died from tower/player
        EndWave();
    }

    private void EndWave()
    {
        bool isLastWave = false;
        if (_currentWaveIndex >= _waves.Length) isLastWave = true;

        OnWaveEnd?.Invoke(_waves[_currentWaveIndex - 1].GoldForWaveCompleated, _currentWaveIndex, isLastWave);      
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        _currentWaveIndex = 0;
        _currentMiniWaveIndex = 0;
        _waveCompleated = false;
    }
}
