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
        private CircleCollider2D _collider;
        private Vector3 _originalLocalScale;
        private float _originalColliderRadius;
        private Vector2 _originalColliderOffset;
        private string _originalTag;
        private IDisposable _returnSubscription;
        private bool _isCollected;
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
            _collider = GetComponent<CircleCollider2D>();
            _originalLocalScale = transform.localScale;
            if (_collider != null)
            {
                _originalColliderRadius = _collider.radius;
                _originalColliderOffset = _collider.offset;
            }
            _originalTag = tag;
            
            _signalBus = signalBus;
            _signalBus.Subscribe<ClearAll>(HeroDeath);
            if (isActiveAndEnabled)
                StartReturnTracking();
        }

        private void OnEnable()
        {
            StartReturnTracking();
        }

        private void StartReturnTracking()
        {
            if (_signalBus == null)
                return;

            _returnSubscription?.Dispose();
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            _returnSubscription = Observable.EveryUpdate()
                .Where(_ => !_isCollected && _mainCamera != null &&
                            _mainCamera.transform.position.y + 7f < transform.position.y &&
                            gameObject.activeInHierarchy)
                .Subscribe(_ => _signalBus.Fire(new BonusIsOut { bonus = this }));
        }

        private void OnDisable()
        {
            _returnSubscription?.Dispose();
            _returnSubscription = null;
        }

        public void Init(BonusType type, int missionItemVariant = 0)
        {
            _isCollected = false;
            _type = type;
            tag = type == BonusType.Acceleration ? "Acceleration" : "Coin";
            ResetVisualGeometry();
            if (_renderer != null)
            {
                _renderer.enabled = true;
                _renderer.sprite = _originalSprite;
            }
            if (_animatorController != null)
                _animatorController.enabled = type != BonusType.MissionItem;

            switch (type)
            {
                case BonusType.Coin:
                    if (_animatorController != null)
                        _animatorController.SetBool(IsCoin, true);
                    break;
                case BonusType.Acceleration:
                    if (_animatorController != null)
                        _animatorController.SetBool(IsCoin, false);
                    break;
                case BonusType.MissionItem:
                    var itemSprite = ChallengeAssetCatalog.LoadMissionItem(missionItemVariant);
                    if (_renderer != null && itemSprite != null)
                    {
                        _renderer.sprite = itemSprite;
                        var visualScale = CollectibleDefinition.GetMissionItemScale(itemSprite);
                        transform.localScale = _originalLocalScale * visualScale;
                        if (_collider != null)
                        {
                            _collider.radius = CollectibleDefinition.GetMissionItemColliderRadius(itemSprite, visualScale);
                            _collider.offset = Vector2.zero;
                        }
                    }
                    break;
            }
        }

        private void HeroDeath()
        {
            if(gameObject.activeSelf && !_isCollected)
                _signalBus.Fire(new BonusIsOut() { bonus = this});
        }

        public bool TryCollect()
        {
            if (_isCollected || !gameObject.activeInHierarchy)
                return false;

            _isCollected = true;
            if (_collider != null)
                _collider.enabled = false;
            return true;
        }

        public void SetPosition(float positionX, float angle, int index)
        {
            LineIndex = index;
            transform.localPosition = new Vector2(positionX, (-9.75f - index * 2) + Mathf.Sin(angle)*positionX);
            if (_collider != null)
                _collider.enabled = !_isCollected;
        }

        public void PrepareForSpawn()
        {
            _isCollected = false;
            LineIndex = -1;
            if (_collider != null)
                _collider.enabled = false;
        }

        public void Clear()
        {
            _isCollected = false;
            LineIndex = -1;
            tag = _originalTag;
            ResetVisualGeometry();
            if (_collider != null)
                _collider.enabled = false;
            if (_renderer != null)
            {
                _renderer.enabled = true;
                _renderer.sprite = _originalSprite;
            }
            if (_animatorController != null)
            {
                _animatorController.enabled = true;
                _animatorController.SetBool(IsCoin, true);
            }
        }

        private void ResetVisualGeometry()
        {
            transform.localScale = _originalLocalScale;
            if (_collider == null)
                return;

            _collider.radius = _originalColliderRadius;
            _collider.offset = _originalColliderOffset;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _returnSubscription?.Dispose();
        }
    }
}
