using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Installers;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

[DefaultExecutionOrder(-100)]
public class CameraController : MonoBehaviour
{
    [SerializeField] private PixelPerfectCamera _pixelPerfectCamera;

    [Inject] private HeroController _heroController;
    [Inject] private UIController _uiController;
    [Inject] private SoInstaller.GameSettings _gameSettings;
    
    private SignalBus _signalBus;
    private Camera _camera;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;

        _signalBus.Subscribe<ClearAll>(x => transform.localPosition = new Vector3(0, .5f, -10));
        _signalBus.Subscribe<HeroDie>(Shake);
    }

    private void Shake()
    {
        transform.DOShakePosition(.5f, .5f);
    }

    public void ForceSnapToHero()
    {
        if (_heroController == null || _gameSettings == null)
            return;

        transform.DOKill();
        var targetY = _heroController.transform.position.y - _gameSettings.offset.y;
        transform.position = new Vector3(0f, targetY, transform.position.z);
    }

    private void Start()
    {
        _camera = GetComponent<Camera>();
        var koeff = (float)Screen.height / Screen.width;
        _pixelPerfectCamera.refResolutionY = (int)(_pixelPerfectCamera.refResolutionX*koeff);
        _pixelPerfectCamera.pixelSnapping = true;
        _camera.orthographicSize = _camera.orthographicSize*((1080f/1920f)/((float)Screen.width/(float)Screen.height));
    }

    private void LateUpdate()
    {
        // Scene services can be injected after the camera's first frame when
        // entering Play Mode from the editor.
        if (_uiController == null || _heroController == null || _gameSettings == null ||
            !_uiController.IsGame || !_heroController.gameObject.activeInHierarchy)
            return;

        var targetY = _heroController.transform.position.y - _gameSettings.offset.y;
        if (targetY >= transform.position.y)
            return;

        transform.position = new Vector3(0f, targetY, transform.position.z);
    }
}
