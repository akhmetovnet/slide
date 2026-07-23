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

    private void Start()
    {
        _compositeDisposable = new CompositeDisposable();
        _currentIndex = 1;
        
        Move(true, 0f);
        
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
            _leftWalls[i].DOLocalMoveX(_leftWalls[i].localPosition.x + (isOut ? -Width : Width), time);
            if (isOut)
                _leftWalls[i].DOLocalMoveY(-3.35f - i * 6.4f, time);
        }
        
        for (var i = 0; i < _rightWalls.Length; i++)
        {
            _rightWalls[i].DOLocalMoveX(_rightWalls[i].localPosition.x + (isOut ? Width : -Width), time);
            if (isOut)
                _rightWalls[i].DOLocalMoveY(-3.35f - i * 6.4f, time);
        }
    }

    private void OnDisable()
    {
        _compositeDisposable?.Dispose();
    }
}
