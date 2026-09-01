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
        [SerializeField] private Sprite[] _dischargeFrames;
        
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
        private Collider2D[] _thorn0Colliders;
        private Collider2D[] _thorn1Colliders;
        private Collider2D[] _thorn2Colliders;
        private Collider2D[] _longLightningColliders;
        private Rigidbody2D _thorn0Rigidbody;
    
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
            PrepareForSpawn();
            if (_thorn0Rigidbody != null)
                _thorn0Rigidbody.simulated = true;
            var fieldWidth = _settings.fieldWidht;
            var positionX = _random.NextFloat() * (fieldWidth * 2) - fieldWidth;
            transform.localPosition = new Vector3(
                positionX,
                -11f - index * 2f + Mathf.Sin(lineAngle) * positionX,
                0f);
            var type = ThornType.None;
            var locationConfig = LocationCatalog.GetActive(_gameController);
            var isFutureCity = FutureCityTheme.IsActive(_gameController);
            var challenge = _gameController.CurrentChallengeDefinition;
            type = challenge != null
                ? SelectWeightedType(challenge.HazardWeights, _random.NextFloat(), locationConfig)
                : SelectLegacyType(isFutureCity);
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

            ApplyLocationVisual(type, locationConfig);
            var speedMultiplier = challenge?.ObstacleSpeedMultiplier ?? 1f;
            
            switch (type)
            {
                case ThornType.Static:
                    SetHazardActive(_thorn0, _thorn0Colliders, true);
                    SetHazardActive(_thorn1, _thorn1Colliders, false);
                    SetHazardActive(_thorn2, _thorn2Colliders, false);
                    SetHazardActive(_longLightning, _longLightningColliders, false);
                    break;
                case ThornType.Kinematic:
                case ThornType.Drone:
                    SetHazardActive(_thorn0, _thorn0Colliders, true);
                    SetHazardActive(_thorn1, _thorn1Colliders, false);
                    SetHazardActive(_thorn2, _thorn2Colliders, false);
                    SetHazardActive(_longLightning, _longLightningColliders, false);
                    
                    var kinematicOffset = 0.0f;
                    if (transform.localPosition.x < 0)
                        kinematicOffset = .5f;
                    else if (transform.localPosition.x >= 0)
                        kinematicOffset = -.5f;
                    
                    StartHorizontalMovement(kinematicOffset, speedMultiplier);
                    break;
                case ThornType.Laser:
                    transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
                    
                    SetHazardActive(_thorn0, _thorn0Colliders, false);
                    SetHazardActive(_thorn1, _thorn1Colliders, true);
                    SetHazardActive(_thorn2, _thorn2Colliders, true);
                    SetHazardActive(_longLightning, _longLightningColliders, true);

                    _laserSequence = DOTween.Sequence();
                    _laserSequence.AppendInterval(_thornSettings.onTime / speedMultiplier)
                        .AppendCallback(()=>
                        {
                            SetHazardActive(_longLightning, _longLightningColliders, false);
                        })
                        .AppendInterval(_thornSettings.offTime / speedMultiplier)
                        .AppendCallback(()=>
                        {
                            if (_audioSource != null && _audioSource.isActiveAndEnabled)
                                _audioSource.Play();
                            SetHazardActive(_longLightning, _longLightningColliders, true);
                        })
                        .SetLoops(-1);
                    break;
                case ThornType.RotatingSpikes:
                case ThornType.PopUpSpikes:
                case ThornType.StickySurface:
                case ThornType.RotatingLaser:
                    SetBaseHazardsActive(false);
                    EnsureDynamicHazard();
                    _dynamicHazard.Configure(type, speedMultiplier, locationConfig);
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

        public static ThornType SelectWeightedType(ChallengeHazardWeights weights,
            float normalizedRoll, LocationConfig config = null)
        {
            var types = new[]
            {
                ThornType.Static, ThornType.Kinematic, ThornType.Laser, ThornType.Drone,
                ThornType.RotatingSpikes, ThornType.PopUpSpikes,
                ThornType.StickySurface, ThornType.RotatingLaser
            };
            var total = 0f;
            foreach (var candidate in types)
            {
                if (CanSpawn(candidate, config))
                    total += Mathf.Max(0f, weights.Get(candidate));
            }
            if (total <= 0f)
                return ThornType.Static;

            var roll = Mathf.Clamp01(normalizedRoll) * total;
            foreach (var candidate in types)
            {
                if (!CanSpawn(candidate, config))
                    continue;
                var weight = Mathf.Max(0f, weights.Get(candidate));
                if (weight <= 0f)
                    continue;
                roll -= weight;
                if (roll <= 0f)
                    return candidate;
            }
            return ThornType.Static;
        }

        private static bool CanSpawn(ThornType type, LocationConfig config)
        {
            if (config == null || type == ThornType.Static || type == ThornType.Kinematic ||
                type == ThornType.Laser || type == ThornType.Drone)
                return true;

            var settings = config.Hazards;
            if (settings == null)
                return false;

            switch (type)
            {
                case ThornType.RotatingSpikes:
                    return !string.IsNullOrEmpty(settings.RotatingSpikesVisualPath);
                case ThornType.PopUpSpikes:
                    return !string.IsNullOrEmpty(settings.PopUpSpikesVisualPath);
                case ThornType.StickySurface:
                    return !string.IsNullOrEmpty(settings.StickyVisualPath);
                case ThornType.RotatingLaser:
                    return config.HazardVisuals != null &&
                           !string.IsNullOrEmpty(config.HazardVisuals.BarrierLeftPath) &&
                           !string.IsNullOrEmpty(config.HazardVisuals.BarrierRightPath) &&
                           !string.IsNullOrEmpty(config.HazardVisuals.BarrierVfxPath);
                default:
                    return false;
            }
        }

        private void SetBaseHazardsActive(bool active)
        {
            SetHazardActive(_thorn0, _thorn0Colliders, active);
            SetHazardActive(_thorn1, _thorn1Colliders, active);
            SetHazardActive(_thorn2, _thorn2Colliders, active);
            SetHazardActive(_longLightning, _longLightningColliders, active);
        }

        private static void SetHazardActive(GameObject hazard, Collider2D[] colliders, bool active)
        {
            if (hazard == null)
                return;

            hazard.SetActive(active);
            if (colliders == null)
                return;

            foreach (var collider in colliders)
            {
                if (collider != null)
                    collider.enabled = active;
            }
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
            _thorn0Colliders = _thorn0.GetComponentsInChildren<Collider2D>(true);
            _thorn1Colliders = _thorn1.GetComponentsInChildren<Collider2D>(true);
            _thorn2Colliders = _thorn2.GetComponentsInChildren<Collider2D>(true);
            _longLightningColliders = _longLightning.GetComponentsInChildren<Collider2D>(true);
            _thorn0Rigidbody = _thorn0.GetComponent<Rigidbody2D>();

            var circleLightning = _thorn0.transform.Find("Lightning");
            if (circleLightning != null)
            {
                _circleLightningRenderer = circleLightning.GetComponent<SpriteRenderer>();
                _circleLightningAnimator = circleLightning.GetComponent<Animator>();
                CopySorting(_thorn0Renderer, _circleLightningRenderer, 1);
            }

            _longLightningRenderer = _longLightning.GetComponent<SpriteRenderer>();
            _longLightningAnimator = _longLightning.GetComponent<Animator>();
            CopySorting(_thorn1Renderer, _longLightningRenderer, 1);
        }

        private void ApplyLocationVisual(ThornType type, LocationConfig config)
        {
            if (config == null || config.HazardVisuals == null)
            {
                if (_thorn0Renderer != null) _thorn0Renderer.sprite = _originalThorn0Sprite;
                if (_thorn1Renderer != null) _thorn1Renderer.sprite = _originalThorn1Sprite;
                if (_thorn2Renderer != null) _thorn2Renderer.sprite = _originalThorn2Sprite;
                SetLocationVfx(type, null);
                return;
            }

            var visuals = config.HazardVisuals;
            if (_thorn0Renderer != null)
            {
                var path = type == ThornType.Drone
                    ? visuals.DronePath
                    : type == ThornType.Kinematic ? visuals.MovingBombPath : visuals.StaticBombPath;
                _thorn0Renderer.sprite = LocationTheme.LoadSprite(config, path) ?? _originalThorn0Sprite;
            }
            if (_thorn1Renderer != null)
                _thorn1Renderer.sprite = LocationTheme.LoadSprite(config, visuals.BarrierLeftPath) ??
                                         _originalThorn1Sprite;
            if (_thorn2Renderer != null)
                _thorn2Renderer.sprite = LocationTheme.LoadSprite(config, visuals.BarrierRightPath) ??
                                         _originalThorn2Sprite;

            SetLocationVfx(type, config);
        }

        private void SetLocationVfx(ThornType type, LocationConfig config)
        {
            var visuals = config != null ? config.HazardVisuals : null;
            if (_circleLightningRenderer != null)
            {
                if (_circleLightningAnimator != null) _circleLightningAnimator.enabled = false;
                if (_circleFrameAnimator == null)
                    _circleFrameAnimator = _circleLightningRenderer.gameObject.AddComponent<FutureCityFrameAnimator>();
                var circlePath = visuals == null ? null :
                    type == ThornType.Kinematic || type == ThornType.Drone
                        ? visuals.MovingBombVfxPath
                        : visuals.StaticBombVfxPath;
                var circleFrames = config != null
                    ? LocationTheme.LoadFrames(config, circlePath)
                    : _dischargeFrames;
                if (circleFrames == null || circleFrames.Length == 0)
                    circleFrames = FutureCityTheme.LoadFrames("VFX/Circle");
                _circleFrameAnimator.Play(_circleLightningRenderer, circleFrames, 12f);
            }

            if (_longLightningRenderer != null)
            {
                if (_longLightningAnimator != null) _longLightningAnimator.enabled = false;
                if (_longFrameAnimator == null)
                    _longFrameAnimator = _longLightningRenderer.gameObject.AddComponent<FutureCityFrameAnimator>();
                var longFrames = config != null
                    ? LocationTheme.LoadFrames(config, visuals.BarrierVfxPath)
                    : _dischargeFrames;
                if (longFrames == null || longFrames.Length == 0)
                    longFrames = FutureCityTheme.LoadFrames("VFX/Long");
                _longFrameAnimator.Play(_longLightningRenderer, longFrames, 10f);
            }
        }

        private static void CopySorting(SpriteRenderer source, SpriteRenderer target, int orderOffset)
        {
            if (source == null || target == null)
                return;

            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder + orderOffset;
        }

        private void StartHorizontalMovement(float kinematicOffset, float speedMultiplier)
        {
            _moveSequence?.Kill();
            if (_thorn0Rigidbody == null)
                return;

            _thorn0Rigidbody.simulated = true;
            _moveSequence = DOTween.Sequence();
            var position = transform.position;
            var duration = _thornSettings.speed / Mathf.Max(0.5f, speedMultiplier);
            _moveSequence.Append(_thorn0Rigidbody.DOMove(new Vector2(position.x + kinematicOffset, position.y), duration))
                .Append(_thorn0Rigidbody.DOMove(new Vector2(position.x, position.y), duration))
                .SetLoops(-1);
        }
    
        private IObserver<long> ReturnThorn()
        {
            return Observer.Create<long>(_ => { _signalBus.Fire(new ThornIsOut() { thorn = this }); });
        }

        public void PrepareForSpawn()
        {
            _moveSequence?.Kill();
            _laserSequence?.Kill();
            _moveSequence = null;
            _laserSequence = null;
            _audioSource?.Stop();
            ResetAnimator(_circleLightningAnimator);
            ResetAnimator(_longLightningAnimator);
            SetBaseHazardsActive(false);
            _dynamicHazard?.Clear();
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            _thorn0.transform.localPosition = Vector3.zero;
            _thorn0.transform.localRotation = Quaternion.identity;
            if (_thorn0Rigidbody != null)
            {
                _thorn0Rigidbody.DOKill();
                _thorn0Rigidbody.linearVelocity = Vector2.zero;
                _thorn0Rigidbody.angularVelocity = 0f;
                _thorn0Rigidbody.simulated = false;
            }

            if (_thorn0Renderer != null) _thorn0Renderer.sprite = _originalThorn0Sprite;
            if (_thorn1Renderer != null) _thorn1Renderer.sprite = _originalThorn1Sprite;
            if (_thorn2Renderer != null) _thorn2Renderer.sprite = _originalThorn2Sprite;
            _circleFrameAnimator?.Stop();
            _longFrameAnimator?.Stop();
            _lastType = ThornType.None;
            _countType = 0;
            tag = "Thorn";
        }

        private static void ResetAnimator(Animator animator)
        {
            if (animator == null)
                return;

            if (!animator.gameObject.activeInHierarchy)
            {
                animator.enabled = false;
                return;
            }

            animator.Rebind();
            animator.Update(0f);
            animator.enabled = false;
        }

        public void Clear()
        {
            PrepareForSpawn();
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ClearAll>(HeroDeath);
            _returnSubscription?.Dispose();
        }
    }
}
