using System;
using System.Collections.Generic;
using DG.Tweening;
using Installers;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public enum ThornType
    {
        Static,
        Kinematic,
        Laser,
        Drone,
        RotatingSpikes,
        PopUpSpikes,
        StickySurface,
        RotatingLaser,
        None
    }
    
    public class ThornController : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject _thorn0; 
        [SerializeField] private GameObject _thorn1; 
        [SerializeField] private GameObject _thorn2;
        [SerializeField] private GameObject _longLightning;
        [SerializeField] private AudioSource _audioSource;
        
        [Inject] private SoInstaller.GameSettings _settings;
        [Inject] private GameController _gameController;


        private SignalBus _signalBus;
        private Sequence _moveSequence;
        private Sequence _laserSequence;
        private IDisposable _returnSubscription;
        private Camera _mainCamera;
        private SoInstaller.ThornSettings _thornSettings;
        private fgRandom _random;
        private ThornType _lastType;
        private int _countType;
        private SpriteRenderer _thorn0Renderer;
        private SpriteRenderer _thorn1Renderer;
        private SpriteRenderer _thorn2Renderer;
        private Sprite _originalThorn0Sprite;
        private Sprite _originalThorn1Sprite;
        private Sprite _originalThorn2Sprite;
        private SpriteRenderer _circleLightningRenderer;
        private SpriteRenderer _longLightningRenderer;
        private Animator _circleLightningAnimator;
        private Animator _longLightningAnimator;
        private FutureCityFrameAnimator _circleFrameAnimator;
        private FutureCityFrameAnimator _longFrameAnimator;
        private ChallengeHazardRuntime _dynamicHazard;
    
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _mainCamera = Camera.main;
            _thornSettings = _settings.thornSettings;
            _random = new fgRandom((uint)System.DateTime.Now.Ticks);
            _lastType = ThornType.None;
            CacheVisualComponents();
            
            _signalBus = signalBus;
            _signalBus.Subscribe<ClearAll>(HeroDeath);
        }
        
        private void OnEnable()
        {
            _returnSubscription?.Dispose();
            _returnSubscription = Observable.EveryUpdate()
                .Where(_ => _mainCamera != null &&
                            _mainCamera.transform.position.y + 7 < transform.position.y &&
                            gameObject.activeInHierarchy)
                .Subscribe(ReturnThorn());
        }

        private void OnDisable()
        {
            _returnSubscription?.Dispose();
            _returnSubscription = null;
        }

        private void HeroDeath()
        {
            if(gameObject.activeSelf)
                _signalBus.Fire(new ThornIsOut() { thorn = this });
        }

        public void Init(int index, bool isTutorial, float lineAngle = 0f)
        {
            var fieldWidth = _settings.fieldWidht;
            var positionX = _random.NextFloat() * (fieldWidth * 2) - fieldWidth;
            transform.localPosition = new Vector3(
                positionX,
                -11f - index * 2f + Mathf.Sin(lineAngle) * positionX,
                0f);
            var type = ThornType.None;
            var isFutureCity = FutureCityTheme.IsActive(_gameController);
            var challenge = _gameController.CurrentChallengeDefinition;
            type = challenge != null ? SelectWeightedType(challenge.HazardWeights) : SelectLegacyType(isFutureCity);
            _countType = (type == _lastType) ? _countType + 1 : 0;
            _lastType = type;
            if (isTutorial && index < 2)
            {
                type = ThornType.Static;
                positionX = 0.5f;
                transform.localPosition = new Vector3(
                    positionX,
                    -11f - index * 2f + Mathf.Sin(lineAngle) * positionX,
                    0f);
            }

            ApplyLocationVisual(type, isFutureCity);
            var speedMultiplier = challenge?.ObstacleSpeedMultiplier ?? 1f;
            
            switch (type)
            {
                case ThornType.Static:
                    _thorn0.SetActive(true);
                    _thorn1.SetActive(false);
                    _thorn2.SetActive(false);
                    _longLightning.SetActive(false);
                    break;
                case ThornType.Kinematic:
                case ThornType.Drone:
                    _thorn0.SetActive(true);
                    _thorn1.SetActive(false);
                    _thorn2.SetActive(false);
                    _longLightning.SetActive(false);
                    
                    var kinematicOffset = 0.0f;
                    if (transform.localPosition.x < 0)
                        kinematicOffset = .5f;
                    else if (transform.localPosition.x >= 0)
                        kinematicOffset = -.5f;
                    
                    StartHorizontalMovement(kinematicOffset, speedMultiplier);
                    break;
                case ThornType.Laser:
                    transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
                    
                    _thorn0.SetActive(false);
                    _thorn1.SetActive(true);
                    _thorn2.SetActive(true);
                    _longLightning.SetActive(true);

                    _laserSequence = DOTween.Sequence();
                    _laserSequence.AppendInterval(_thornSettings.onTime / speedMultiplier)
                        .AppendCallback(()=>
                        {
                            _longLightning.SetActive(false);
                        })
                        .AppendInterval(_thornSettings.offTime / speedMultiplier)
                        .AppendCallback(()=>
                        {
                            if (_audioSource != null && _audioSource.isActiveAndEnabled)
                                _audioSource.Play();
                            _longLightning.SetActive(true);
                        })
                        .SetLoops(-1);
                    break;
                case ThornType.RotatingSpikes:
                case ThornType.PopUpSpikes:
                case ThornType.StickySurface:
                case ThornType.RotatingLaser:
                    SetBaseHazardsActive(false);
                    EnsureDynamicHazard();
                    _dynamicHazard.Configure(type, speedMultiplier);
                    break;
            }    
        }

        private ThornType SelectLegacyType(bool isFutureCity)
        {
            var typeCount = isFutureCity ? 4 : 3;
            if (_countType < 3)
                return (ThornType)_random.NextInt(0, typeCount);

            var types = new List<int>();
            for (var i = 0; i < typeCount; i++)
                types.Add(i);
            types.Remove((int)_lastType);
            return (ThornType)types[_random.NextInt(0, types.Count)];
        }

        private ThornType SelectWeightedType(ChallengeHazardWeights weights)
        {
            var types = new[]
            {
                ThornType.Static, ThornType.Kinematic, ThornType.Laser, ThornType.Drone,
                ThornType.RotatingSpikes, ThornType.PopUpSpikes,
                ThornType.StickySurface, ThornType.RotatingLaser
            };
            var total = 0f;
            foreach (var candidate in types)
                total += Mathf.Max(0f, weights.Get(candidate));
            if (total <= 0f)
                return ThornType.Static;

            var roll = _random.NextFloat() * total;
            foreach (var candidate in types)
            {
                roll -= Mathf.Max(0f, weights.Get(candidate));
                if (roll <= 0f)
                    return candidate;
            }
            return ThornType.Static;
        }

        private void SetBaseHazardsActive(bool active)
        {
            _thorn0.SetActive(active);
            _thorn1.SetActive(active);
            _thorn2.SetActive(active);
            _longLightning.SetActive(active);
        }

        private void EnsureDynamicHazard()
        {
            if (_dynamicHazard != null)
                return;

            var dynamicObject = new GameObject("Dynamic Challenge Hazard");
            dynamicObject.transform.SetParent(transform, false);
            _dynamicHazard = dynamicObject.AddComponent<ChallengeHazardRuntime>();
            _dynamicHazard.Initialize(_thorn0Renderer);
        }

        private void CacheVisualComponents()
        {
            _thorn0Renderer = _thorn0.GetComponent<SpriteRenderer>();
            _thorn1Renderer = _thorn1.GetComponent<SpriteRenderer>();
            _thorn2Renderer = _thorn2.GetComponent<SpriteRenderer>();
            _originalThorn0Sprite = _thorn0Renderer != null ? _thorn0Renderer.sprite : null;
            _originalThorn1Sprite = _thorn1Renderer != null ? _thorn1Renderer.sprite : null;
            _originalThorn2Sprite = _thorn2Renderer != null ? _thorn2Renderer.sprite : null;

            var circleLightning = _thorn0.transform.Find("Lightning");
            if (circleLightning != null)
            {
                _circleLightningRenderer = circleLightning.GetComponent<SpriteRenderer>();
                _circleLightningAnimator = circleLightning.GetComponent<Animator>();
            }

            _longLightningRenderer = _longLightning.GetComponent<SpriteRenderer>();
            _longLightningAnimator = _longLightning.GetComponent<Animator>();
        }

        private void ApplyLocationVisual(ThornType type, bool isFutureCity)
        {
            if (!isFutureCity)
            {
                if (_thorn0Renderer != null) _thorn0Renderer.sprite = _originalThorn0Sprite;
                if (_thorn1Renderer != null) _thorn1Renderer.sprite = _originalThorn1Sprite;
                if (_thorn2Renderer != null) _thorn2Renderer.sprite = _originalThorn2Sprite;
                SetFutureCityVfx(false);
                return;
            }

            if (_thorn0Renderer != null)
            {
                _thorn0Renderer.sprite = type == ThornType.Drone
                    ? FutureCityTheme.LoadSprite("Enemies/enemy_drone")
                    : FutureCityTheme.LoadSprite("Enemies/enemy_bomb");
            }
            if (_thorn1Renderer != null)
                _thorn1Renderer.sprite = FutureCityTheme.LoadSprite("Enemies/enemy_laser_left");
            if (_thorn2Renderer != null)
                _thorn2Renderer.sprite = FutureCityTheme.LoadSprite("Enemies/enemy_laser_right");
            SetFutureCityVfx(true);
        }

        private void SetFutureCityVfx(bool isFutureCity)
        {
            if (!isFutureCity)
            {
                _circleFrameAnimator?.Stop();
                _longFrameAnimator?.Stop();
                if (_circleLightningAnimator != null) _circleLightningAnimator.enabled = true;
                if (_longLightningAnimator != null) _longLightningAnimator.enabled = true;
                return;
            }

            if (_circleLightningRenderer != null)
            {
                if (_circleLightningAnimator != null) _circleLightningAnimator.enabled = false;
                if (_circleFrameAnimator == null)
                    _circleFrameAnimator = _circleLightningRenderer.gameObject.AddComponent<FutureCityFrameAnimator>();
                _circleFrameAnimator.Play(_circleLightningRenderer,
                    FutureCityTheme.LoadFrames("VFX/Circle"), 12f);
            }

            if (_longLightningRenderer != null)
            {
                if (_longLightningAnimator != null) _longLightningAnimator.enabled = false;
                if (_longFrameAnimator == null)
                    _longFrameAnimator = _longLightningRenderer.gameObject.AddComponent<FutureCityFrameAnimator>();
                _longFrameAnimator.Play(_longLightningRenderer,
                    FutureCityTheme.LoadFrames("VFX/Long"), 10f);
            }
        }

        private void StartHorizontalMovement(float kinematicOffset, float speedMultiplier)
        {
            _moveSequence = DOTween.Sequence();
            var position = transform.position;
            var rigidbody = _thorn0.GetComponent<Rigidbody2D>();
            var duration = _thornSettings.speed / Mathf.Max(0.5f, speedMultiplier);
            _moveSequence.Append(rigidbody.DOMove(new Vector2(position.x + kinematicOffset, position.y), duration))
                .Append(rigidbody.DOMove(new Vector2(position.x, position.y), duration))
                .SetLoops(-1);
        }
    
        private IObserver<long> ReturnThorn()
        {
            return Observer.Create<long>(_ => { _signalBus.Fire(new ThornIsOut() { thorn = this }); });
        }

        public void Clear()
        {
            _moveSequence?.Kill();
            _laserSequence?.Kill();
            _moveSequence = null;
            _laserSequence = null;
            _dynamicHazard?.Clear();
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _thorn0.transform.localPosition = Vector3.zero;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ClearAll>(HeroDeath);
            _returnSubscription?.Dispose();
        }
    }
}
