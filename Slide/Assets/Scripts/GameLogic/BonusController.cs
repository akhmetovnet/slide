using System;
using Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace GameLogic
{
    public enum BonusType
    {
        Coin,
        Acceleration,
        MissionItem
    }
    
    public class BonusController : MonoBehaviour, IDisposable
    {
        [SerializeField] private Animator _animatorController;
    
        private SignalBus _signalBus;
        private CompositeDisposable _compositeDisposable;
        private Camera _mainCamera;
        private BonusType _type;
        private SpriteRenderer _renderer;
        private Sprite _originalSprite;
        private static readonly int IsCoin = Animator.StringToHash("IsCoin");

        public BonusType Type => _type;
        public int LineIndex { get; private set; }

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _mainCamera = Camera.main;
            _compositeDisposable = new CompositeDisposable();
            _renderer = GetComponent<SpriteRenderer>();
            _originalSprite = _renderer != null ? _renderer.sprite : null;
            
            _signalBus = signalBus;
            _signalBus.Subscribe<ClearAll>(HeroDeath);
        }

        public void Init(BonusType type, int missionItemVariant = 0)
        {
            _type = type;
            tag = type == BonusType.Acceleration ? "Acceleration" : "Coin";
            if (_animatorController != null)
                _animatorController.enabled = type != BonusType.MissionItem;

            switch (type)
            {
                case BonusType.Coin:
                    _animatorController.SetBool(IsCoin, true);
                    break;
                case BonusType.Acceleration:
                    _animatorController.SetBool(IsCoin, false);
                    break;
                case BonusType.MissionItem:
                    var itemSprite = ChallengeAssetCatalog.LoadMissionItem(missionItemVariant);
                    if (_renderer != null && itemSprite != null)
                        _renderer.sprite = itemSprite;
                    break;
            }
        }

        private void HeroDeath()
        {
            if(gameObject.activeSelf)
                _signalBus.Fire(new BonusIsOut() { bonus = this});
        }

        public void SetPosition(float positionX, float angle, int index)
        {
            LineIndex = index;
            transform.localPosition = new Vector2(positionX, (-9.75f - index * 2) + Mathf.Sin(angle)*positionX);
        }

        public void Clear()
        {
            LineIndex = -1;
            if (_renderer != null)
                _renderer.sprite = _originalSprite;
            if (_animatorController != null)
                _animatorController.enabled = true;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
