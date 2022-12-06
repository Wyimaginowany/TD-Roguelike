﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _damage = 25f;
    [SerializeField] private float _speed = 50f;
    [SerializeField] private float _rotateSpeed = 0.1f;
    [SerializeField] private float _lookDirectionOffset = 90f;

    public static event Action<float> OnEnemyPathCompleate;

    private WaypointManager _waypointManager;
    private float _distanceToNextWaypoint = 0f;
    private int _currentWaypoint;

    private void Start()
    {
        _waypointManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<WaypointManager>();
    }

    private void Update()
    {
        RotateTowardsNextWaypoint();
        MoveTowardsNextWaypoint();
    }

    private void MoveTowardsNextWaypoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, _waypointManager.Waypoints[_currentWaypoint].position, _speed * Time.deltaTime);

        _distanceToNextWaypoint = Vector3.Distance(transform.position, _waypointManager.Waypoints[_currentWaypoint].position);

        if (_distanceToNextWaypoint < 0.01f)
        {
            if (_currentWaypoint < _waypointManager.Waypoints.Length - 1)
            {
                _currentWaypoint++;
                _distanceToNextWaypoint = Vector3.Distance(transform.position, _waypointManager.Waypoints[_currentWaypoint].position);
            }
            else
            {
                OnEnemyPathCompleate?.Invoke(_damage);
                Destroy(gameObject);
            }
        }
    }

    private void RotateTowardsNextWaypoint()
    {
        Vector3 lookDirection = _waypointManager.Waypoints[_currentWaypoint].position - transform.position;
        float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg - _lookDirectionOffset;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.up), _rotateSpeed);
    }

    public int GetCurrentWaypoint()
    {
        return _currentWaypoint;
    }

    public float GetDistanceToNextWaypoint()
    {
        return _distanceToNextWaypoint;
    }
}