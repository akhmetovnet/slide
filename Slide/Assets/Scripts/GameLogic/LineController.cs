using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;

namespace GameLogic
{
	public class LineController : MonoBehaviour, IDisposable
	{
		private const float DefaultAngle = 8.0f;
		
		[SerializeField] private BoxCollider2D _collider;

		[Inject] private GameController _gameController;

		private float _angle;
		private SignalBus _signalBus;
		private IDisposable _returnSubscription;
		private Transform _transform;
		private Camera _mainCamera;
		private ThornController _thorn;
		private LineController _nextLine;
		private SpriteRenderer _renderer;
		private Sprite _originalSprite;
		private bool _originalFlipX;
		private Quaternion _originalColliderRotation;
		private float _defaultAngle;
		private float _currentAngle;
		private Sprite[] _cityPlatforms;
		private ChallengeLocation _platformLocation;
		private Sprite[] _rocketFrames;
		private GameObject[] _rockets;
		private SpriteRenderer[] _rocketRenderers;
		private FutureCityFrameAnimator[] _rocketAnimations;
		private Tween _movementTween;

		public float Angle => Mathf.PI*AngleDegree/180;
		public float AngleDegree => _transform.localScale.x * _currentAngle;
		public BoxCollider2D Collider => _collider;
	
		[Inject]
		public void Construct(SignalBus signalBus)
		{
			_transform = GetComponent<Transform>();
			_renderer = GetComponent<SpriteRenderer>();
			_originalSprite = _renderer != null ? _renderer.sprite : null;
			_originalFlipX = _renderer != null && _renderer.flipX;
			_originalColliderRotation = _collider.transform.localRotation;
			_defaultAngle = Mathf.Abs(Mathf.DeltaAngle(0f, _collider.transform.localEulerAngles.z));
			if (_defaultAngle < 0.1f)
				_defaultAngle = DefaultAngle;
			_currentAngle = _defaultAngle;
			_mainCamera = Camera.main;
			_signalBus = signalBus;
			_signalBus.Subscribe<ClearAll>(HeroDeath);
		}

		private void OnEnable()
		{
			_returnSubscription?.Dispose();
			_returnSubscription = Observable.EveryUpdate()
				.Where(_ => _mainCamera != null &&
				            _mainCamera.transform.position.y + 7 < _transform.position.y &&
				            gameObject.activeInHierarchy)
				.Subscribe(ReturnLine());
		}

		private void OnDisable()
		{
			_returnSubscription?.Dispose();
			_returnSubscription = null;
			StopCityMovement();
		}

		private void HeroDeath()
		{
			if (gameObject.activeSelf)
				_signalBus.Fire(new LineIsOut() { line = this, isForce = true});
		}

		public Vector2 GetDirection()
		{
			_angle = Mathf.PI*AngleDegree/180;
			var vector = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle));
		
			return AngleDegree < 0 ? vector : vector * -1;
		}
		
		public void SetPositionAndRotation(int index, bool isLast)
		{
			StopCityMovement();
			_collider.isTrigger = false;
			_transform.localRotation = Quaternion.identity;
			_transform.localPosition = new Vector3(0, -10 - index * 2, 0);
			var scaleX = (index % 2) == 0 ? -1 : 1;
			_transform.localScale = new Vector3(scaleX, 1, 1);
			ResetPlatformGeometry();
			ApplyLocationVisual(index, isLast);
		}

		private void ApplyLocationVisual(int index, bool isLast)
		{
			if (_renderer == null)
				return;

			var isFutureCity = FutureCityTheme.IsActive(_gameController);
			var isJungle = JungleTheme.IsActive(_gameController);
			if (!isFutureCity && !isJungle)
			{
				_renderer.sprite = _originalSprite;
				ResetPlatformGeometry();
				SetRocketsActive(false);
				return;
			}

			var location = isJungle ? ChallengeLocation.Jungle : ChallengeLocation.FutureCity;
			if (_cityPlatforms == null || _cityPlatforms.Length == 0 || _platformLocation != location)
			{
				_cityPlatforms = isJungle ? JungleTheme.LoadPlatformFrames() : FutureCityTheme.LoadFrames("Platforms");
				_platformLocation = location;
			}
			var challenge = _gameController.CurrentChallengeDefinition;
			if (_cityPlatforms.Length > 0)
			{
				var availableVariants = isJungle ? _cityPlatforms.Length : challenge == null || challenge.Level <= 3
					? 1
					: challenge.Level <= 7 ? 2 : _cityPlatforms.Length;
				availableVariants = Mathf.Clamp(availableVariants, 1, _cityPlatforms.Length);
				_renderer.sprite = _cityPlatforms[Mathf.Abs(index) % availableVariants];
				ApplyCityPlatformGeometry(_renderer.sprite);
			}

			var movingChance = challenge?.MovingPlatformChance ?? 0.12f;
			var deterministicRoll = Mathf.Abs(Mathf.Sin((index + 1) * 12.9898f));
			var isMovingPlatform = index > 1 && !isLast && deterministicRoll < movingChance;
			var rocketFrames = isJungle ? JungleTheme.LoadRocketFrames() : FutureCityTheme.LoadFrames("Rockets");
			SetRocketsActive(isMovingPlatform, rocketFrames);
			if (!isMovingPlatform)
				return;

			var startX = _transform.localPosition.x;
			var movementDuration = Mathf.Lerp(2f, 1.05f,
				Mathf.Clamp01((challenge?.Level ?? 1) / (float)ChallengeLevelCatalog.LevelCount));
			_movementTween = _transform.DOLocalMoveX(startX + 0.22f, movementDuration)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);
		}

		private void ResetPlatformGeometry()
		{
			_currentAngle = _defaultAngle;
			_collider.transform.localRotation = _originalColliderRotation;
			if (_renderer != null)
				_renderer.flipX = _originalFlipX;
		}

		private void ApplyCityPlatformGeometry(Sprite sprite)
		{
			_currentAngle = _defaultAngle;
			_renderer.flipX = _originalFlipX;
			var spriteName = sprite != null ? sprite.name : string.Empty;
			if (spriteName.StartsWith("platform_1", StringComparison.Ordinal))
			{
				_currentAngle = 7.12f;
			}
			else if (spriteName.StartsWith("platform_2", StringComparison.Ordinal))
			{
				_currentAngle = 14.04f;
				_renderer.flipX = !_originalFlipX;
			}
			else if (spriteName.StartsWith("platform_3", StringComparison.Ordinal))
			{
				_currentAngle = 26.56f;
			}

			_collider.transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
		}

		private void SetRocketsActive(bool isActive, Sprite[] frames = null)
		{
			if (!isActive && _rockets == null)
				return;

			if (_rockets == null)
				CreateRockets(frames);

			for (var i = 0; i < _rockets.Length; i++)
			{
				_rockets[i].SetActive(isActive);
				if (isActive && frames != null && frames.Length > 0)
					_rocketAnimations[i].Play(_rocketRenderers[i], frames, 12f,
						i * 0.08f);
			}
		}

		private void CreateRockets(Sprite[] frames)
		{
			_rocketFrames = frames ?? Array.Empty<Sprite>();
			_rockets = new GameObject[2];
			_rocketRenderers = new SpriteRenderer[_rockets.Length];
			_rocketAnimations = new FutureCityFrameAnimator[_rockets.Length];
			for (var i = 0; i < _rockets.Length; i++)
			{
				var rocket = new GameObject("Platform Rocket " + (i + 1));
				rocket.transform.SetParent(_transform, false);
				rocket.transform.localPosition = new Vector3(i == 0 ? -1.08f : 1.08f, -0.34f, 0f);
				var renderer = rocket.AddComponent<SpriteRenderer>();
				renderer.sortingLayerID = _renderer.sortingLayerID;
				renderer.sortingOrder = _renderer.sortingOrder + 1;
				var animation = rocket.AddComponent<FutureCityFrameAnimator>();
				if (_rocketFrames.Length > 0)
					animation.Play(renderer, _rocketFrames, 12f, i * 0.08f);
				_rocketRenderers[i] = renderer;
				_rocketAnimations[i] = animation;
				_rockets[i] = rocket;
			}
		}

		private void StopCityMovement()
		{
			_movementTween?.Kill();
			_movementTween = null;
			SetRocketsActive(false);
		}

		private IObserver<long> ReturnLine()
		{
			return Observer.Create<long>(_ => { _signalBus.Fire(new LineIsOut() {line = this, isForce = false}); });
		}
	
		public void Dispose()
		{
			_signalBus.Unsubscribe<ClearAll>(HeroDeath);
			_returnSubscription?.Dispose();
			StopCityMovement();
		}

		public void AddThorn(ThornController thorn)
		{
			_thorn = thorn;
		}

		public void AddNextLine(LineController line)
		{
			_nextLine = line;
		}

		public LineController GetNextLine()
		{
			return _nextLine;
		}

		public void RemoveThorn()
		{
			 _signalBus.Fire(new ThornIsOut() { thorn = _thorn });
		}
	}
}
