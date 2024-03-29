using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObstacleManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("To Attach")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _buyButton;
    [SerializeField] private TMP_Text _obstacleNameText;
    [SerializeField] private TMP_Text[] _obstaclePriceTexts;
    [SerializeField] private RawImage _iconImage;

    [Header("To Attach")]
    [SerializeField] private AudioClip _removeObstacleSound;
    [SerializeField] private AudioClip _showUISound;


    private AudioSource _audioSource;
    private TowerManager _towerManager;
    private Controls _controls;
    private Obstacle _currentObstacle;

    private void Start()
    {
        _controls = new Controls();
        _controls.Player.Enable();
        
        _towerManager = GameObject.FindGameObjectWithTag("TowerManager").GetComponent<TowerManager>();
        _audioSource = GetComponent<AudioSource>();

        _controls.Player.Info.performed += HandlePlayerMouseInfo;
        TowerManager.OnTowerPlaced += HideUI;
        TowerManager.OnTowerSelectedToPlace += HandleTowerSelect;
        TowerUIManager.OnTowerSelectionMenuShow += HideObstacleMenu;
        PauseManager.OnGamePaused += HideAndDeactiveObstacleMenu;
        PauseManager.OnGameResumed += ShowObstacleMenu;
        PlayerUpgradesManager.OnUpgradeMenuShow += HideAndDeactiveObstacleMenu;
        PlayerUpgradesManager.OnUpgradeMenuHide += ShowObstacleMenu;
        PlayerBase.OnBaseDestroyed += HandleGameOver;
        PlayerHealth.OnPlayerDeath += HandleGameOver;
    }

    private void OnDestroy()
    {
        _controls.Player.Info.performed -= HandlePlayerMouseInfo;
        TowerManager.OnTowerPlaced -= HideUI;
        TowerManager.OnTowerSelectedToPlace -= HandleTowerSelect;
        TowerUIManager.OnTowerSelectionMenuShow -= HideObstacleMenu;
        PauseManager.OnGamePaused -= HideAndDeactiveObstacleMenu;
        PauseManager.OnGameResumed -= ShowObstacleMenu;
        PlayerUpgradesManager.OnUpgradeMenuShow -= HideAndDeactiveObstacleMenu;
        PlayerUpgradesManager.OnUpgradeMenuHide -= ShowObstacleMenu;
        PlayerBase.OnBaseDestroyed -= HandleGameOver;
        PlayerHealth.OnPlayerDeath -= HandleGameOver;
    }

    private void HideAndDeactiveObstacleMenu()
    {
        if (_animator.GetBool("shown")) this.gameObject.SetActive(false);
    }

    private void ShowObstacleMenu()
    {
        if (!this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
            _animator.SetBool("shown", true);
            _audioSource.PlayOneShot(_showUISound);
        }
    }

    private void HandlePlayerMouseInfo(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0) return;

        if (EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit obstacleHit;

        if (Physics.Raycast(ray, out obstacleHit, Mathf.Infinity, _obstacleLayer))
        {
            GameObject obstacleHitGameObject = obstacleHit.transform.gameObject;
            Obstacle obstacle = obstacleHitGameObject.GetComponent<Obstacle>();

            if (obstacle != null)
            {
                UpdateCanvasInfo(obstacle);

                if (obstacle.GetRemovePrice() <= _towerManager.GetCurrentMoneyAmount()) _buyButton.SetActive(true);
                else _buyButton.SetActive(false);

                if (_currentObstacle == obstacle) return;
                _animator.Play("ObstacleMenuHidden", 0);
                _animator.SetBool("shown", true);
                _audioSource.PlayOneShot(_showUISound);
                _currentObstacle = obstacle;
                return;
            }

        }
            
        _currentObstacle = null;
        _animator.SetBool("shown", false);
    }

    private void UpdateCanvasInfo(Obstacle obstacle)
    {
        _obstacleNameText.text = obstacle.GetObstacleData().ObstacleName;
        _iconImage.texture = obstacle.GetObstacleData().ObstacleIcon;

        foreach (TMP_Text priceText in _obstaclePriceTexts)
        {
            priceText.text = obstacle.GetRemovePrice().ToString();
        }
    }

    private void HideUI(Tower tower)
    {
        _animator.SetBool("shown", false);
    }

    public void CloseButton()
    {
        _animator.SetBool("shown", false);
    }

    private void HandleTowerSelect(Tower tower)
    {
        _animator.SetBool("shown", false);
    }

    private void HideObstacleMenu()
    {
        _animator.SetBool("shown", false);
    }

    public void DestroyObstacle()
    {
        if (_currentObstacle != null)
        {
            _towerManager.RemoveObstacle(_currentObstacle.GetRemovePrice());
            _currentObstacle.Remove();
            _currentObstacle = null;
            _animator.SetBool("shown", false);
            _audioSource.PlayOneShot(_removeObstacleSound);
        }
    }

    private void HandleGameOver()
    {
        _controls.Player.Info.performed -= HandlePlayerMouseInfo;
        PauseManager.OnGamePaused -= HideAndDeactiveObstacleMenu;
        PauseManager.OnGameResumed -= ShowObstacleMenu;
    }
}
