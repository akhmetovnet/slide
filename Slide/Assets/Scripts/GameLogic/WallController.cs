using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

public class WallController : MonoBehaviour
{
    private const float Width = 1;
    private const float Height = 6.4f;
    
    [SerializeField] private Transform[] _leftWalls;
    [SerializeField] private Transform[] _rightWalls;


    [Inject] private HeroController _heroController;

    private int _currentIndex;
    private CompositeDisposable _compositeDisposable;
    private Vector3[] _leftWallStartPositions;
    private Vector3[] _rightWallStartPositions;
    private Vector2 _startAreaOffset;
    private bool _startWallsAreOut = true;
    private bool _hasStarted;
    private bool _startAreaConfigured;

    private void Awake()
    {
        _leftWallStartPositions = CaptureStartPositions(_leftWalls);
        _rightWallStartPositions = CaptureStartPositions(_rightWalls);
    }

    private void Start()
    {
        _compositeDisposable = new CompositeDisposable();
        _currentIndex = 1;
        _hasStarted = true;
        
        Move(_startWallsAreOut, 0f);
        
        Observable.EveryUpdate()
            .Where(_ => _heroController.transform.position.y < _leftWalls[_currentIndex].position.y)
            .Subscribe(UpdatePosition())
            .AddTo(_compositeDisposable);
    }

    private IObserver<long> UpdatePosition()
    {
        return Observer.Create<long>(observer =>
        {
            var oldIndex = _currentIndex;
            _currentIndex++;
            if (_currentIndex >= _leftWalls.Length)
                _currentIndex = 0;
            
            _leftWalls[_currentIndex].localPosition = new Vector3(_leftWalls[oldIndex].localPosition.x, _leftWalls[oldIndex].localPosition.y - Height, _leftWalls[oldIndex].localPosition.z);
            _rightWalls[_currentIndex].localPosition = new Vector3(_rightWalls[oldIndex].localPosition.x, _rightWalls[oldIndex].localPosition.y - Height, _rightWalls[oldIndex].localPosition.z);
        });
    }

    public void Move(bool isOut, float time)
    {
        for (var i = 0; i < _leftWalls.Length; i++)
        {
            var targetPosition = _leftWallStartPositions[i] + (Vector3)_startAreaOffset;
            if (isOut)
                targetPosition.x -= Width;
            MoveWall(_leftWalls[i], targetPosition, time);
        }
        
        for (var i = 0; i < _rightWalls.Length; i++)
        {
            var targetPosition = _rightWallStartPositions[i] + (Vector3)_startAreaOffset;
            if (isOut)
                targetPosition.x += Width;
            MoveWall(_rightWalls[i], targetPosition, time);
        }
    }

    public void ConfigureStartArea(Vector2 offset, bool wallsAreOut)
    {
        var changed = !_startAreaConfigured || _startAreaOffset != offset ||
                      _startWallsAreOut != wallsAreOut;
        _startAreaOffset = offset;
        _startWallsAreOut = wallsAreOut;
        _startAreaConfigured = true;

        if (_hasStarted && changed)
            Move(_startWallsAreOut, 0f);
    }

    private static Vector3[] CaptureStartPositions(Transform[] walls)
    {
        var positions = new Vector3[walls.Length];
        for (var i = 0; i < walls.Length; i++)
            positions[i] = walls[i].localPosition;
        return positions;
    }

    private static void MoveWall(Transform wall, Vector3 targetPosition, float time)
    {
        wall.DOKill();
        if (time <= 0f)
            wall.localPosition = targetPosition;
        else
            wall.DOLocalMove(targetPosition, time);
    }

    private void OnDisable()
    {
        _compositeDisposable?.Dispose();
    }
}
