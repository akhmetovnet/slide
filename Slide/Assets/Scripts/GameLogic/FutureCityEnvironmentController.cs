using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    [DefaultExecutionOrder(100)]
    public sealed class FutureCityEnvironmentController : MonoBehaviour
    {
        private const string RootName = "[Location Environment]";
        private const string BackgroundLayer = "Background";
        private const float HorizontalLimit = 1.85f;
        private const float FutureCityCarDirection = 1f;
        private const float FutureCityCityBaselineY = 0f;
        private const float FutureCityCityVerticalSpeed = 0.12f;
        private const float FutureCityCityRepeatHeight = 8.05f;
        private const float FutureCityCity1OffsetY = 0f;
        private const float FutureCityCity2OffsetY = -1.65f;
        private const float FutureCityCity3OffsetY = -2.3f;
        private const float FutureCityCity4OffsetY = -0.9f;
        private const float FutureCityCloudVerticalSpeed = FutureCityCityVerticalSpeed;
        private const float FutureCityCloudRepeatHeight = FutureCityCityRepeatHeight;
        private const float FutureCityFarCloudOffsetY = 3.2f;
        private const float FutureCityNearCloudOffsetY = -3.2f;

        private readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();
        private readonly List<AmbientActor> _actors = new List<AmbientActor>();
        private readonly List<SceneThemeEntry> _themeEntries = new List<SceneThemeEntry>();
        private readonly HashSet<EntityId> _themedRendererIds = new HashSet<EntityId>();
        private readonly List<WallLightning> _wallLightnings = new List<WallLightning>();
        private readonly HashSet<Transform> _wallLightningTransforms = new HashSet<Transform>();

        private GameController _gameController;
        private Camera _camera;
        private GameObject _content;
        private bool _isLocationActive;
        private float _activationCameraY;
        private System.Random _random;
        private ChallengeLocation _environmentLocation;
        private LocationConfig _environmentConfig;

        public static FutureCityEnvironmentController Create(GameController gameController)
        {
            var existing = FindAnyObjectByType<FutureCityEnvironmentController>();
            if (existing != null)
            {
                existing.Initialize(gameController);
                return existing;
            }

            var root = new GameObject(RootName);
            var controller = root.AddComponent<FutureCityEnvironmentController>();
            controller.Initialize(gameController);
            return controller;
        }

        public void Refresh()
        {
            EnsureEnvironmentForCurrentLocation();
            SetLocationActive(IsEnvironmentActive());
        }

        private void Initialize(GameController gameController)
        {
            _gameController = gameController;
            _camera = Camera.main;
            _random = new System.Random(6010);

            Refresh();
        }

        private void EnsureEnvironmentForCurrentLocation()
        {
            var location = GetEnvironmentLocation();
            if (_content != null && location == _environmentLocation)
                return;

            SetSceneTheme(null);
            ApplyWallLightningOffsets(null);
            _isLocationActive = false;
            if (_content != null)
            {
                _content.SetActive(false);
                Destroy(_content);
                _content = null;
            }

            _layers.Clear();
            _actors.Clear();
            _environmentConfig = null;
            BuildEnvironment(location);
        }

        private void BuildEnvironment(ChallengeLocation location)
        {
            _environmentLocation = location;
            _environmentConfig = LocationCatalog.Get(location);
            _content = new GameObject("Content");
            _content.transform.SetParent(transform, false);

            if (_environmentConfig != null && _environmentConfig.EnvironmentLayers != null &&
                _environmentConfig.EnvironmentLayers.Length > 0)
            {
                BuildConfiguredEnvironment(_environmentConfig);
                if (location == ChallengeLocation.FutureCity)
                    BuildAmbientActors();
            }
            else if (location == ChallengeLocation.FutureCity)
                BuildFutureCityEnvironment();
            else if (location == ChallengeLocation.Jungle)
                BuildJungleEnvironment();

            _content.SetActive(false);
        }

        private void BuildConfiguredEnvironment(LocationConfig config)
        {
            foreach (var layer in config.EnvironmentLayers)
            {
                if (layer == null || string.IsNullOrEmpty(layer.ResourcePath))
                    continue;

                var repeatMultiplier = layer.VerticalRepeatMultiplier;
                if (layer.VerticalRepeatHeight > 0f)
                {
                    var sprite = LocationTheme.LoadSprite(config, layer.ResourcePath);
                    if (sprite != null && sprite.bounds.size.y > 0f)
                        repeatMultiplier = layer.VerticalRepeatHeight / sprite.bounds.size.y;
                }

                AddLayer(layer.Name, layer.ResourcePath, layer.VerticalSpeed,
                    layer.HorizontalSpeed, layer.SortingOrder, layer.Alpha, layer.Offset,
                    repeatMultiplier, layer.AlignBottomToBaseline);
            }
        }

        private void BuildFutureCityEnvironment()
        {
            AddLayer("Sky", "Environment/sky", 0.015f, 0f, 0, 1f);
            AddFutureCityCityLayer("Far City", "Environment/city_4", 10, FutureCityCity4OffsetY);
            AddFutureCityCityLayer("Mid City A", "Environment/city_3", 20, FutureCityCity3OffsetY);
            AddFutureCityCloudLayer("Far Clouds", "Environment/clouds_far", 0.27f, 1, 0.72f,
                FutureCityFarCloudOffsetY);
            AddFutureCityCityLayer("Mid City B", "Environment/city_2", 30, FutureCityCity2OffsetY);
            AddFutureCityCityLayer("Near City", "Environment/city_1", 40, FutureCityCity1OffsetY);
            AddFutureCityCloudLayer("Near Clouds", "Environment/clouds_near", -0.20f, 2, 0.36f,
                FutureCityNearCloudOffsetY);

            BuildAmbientActors();
        }

        private void AddFutureCityCityLayer(string name, string resourcePath, int sortingOrder,
            float baselineOffsetY)
        {
            var sprite = LoadEnvironmentSprite(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning("Location sprite is missing: " + resourcePath);
                return;
            }

            AddLayer(name, resourcePath, FutureCityCityVerticalSpeed, 0f, sortingOrder, 1f,
                new Vector2(0f, FutureCityCityBaselineY + baselineOffsetY),
                FutureCityCityRepeatHeight / sprite.bounds.size.y, true);
        }

        private void AddFutureCityCloudLayer(string name, string resourcePath, float horizontalSpeed,
            int sortingOrder, float alpha, float baselineOffsetY)
        {
            var sprite = LoadEnvironmentSprite(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning("Location sprite is missing: " + resourcePath);
                return;
            }

            AddLayer(name, resourcePath, FutureCityCloudVerticalSpeed, horizontalSpeed, sortingOrder,
                alpha, new Vector2(0f, FutureCityCityBaselineY + baselineOffsetY),
                FutureCityCloudRepeatHeight / sprite.bounds.size.y, false);
        }

        private void BuildJungleEnvironment()
        {
            var config = JungleTheme.Config;
            if (config == null)
            {
                Debug.LogWarning("Jungle environment config is missing.");
                return;
            }

            foreach (var layer in config.Layers)
            {
                if (layer == null || string.IsNullOrEmpty(layer.ResourcePath))
                    continue;

                var offset = layer.Offset;
                if (layer.AlignBottomToBaseline)
                    offset.y = config.CityBaselineY;

                AddLayer(layer.Name, layer.ResourcePath, layer.VerticalSpeed,
                    layer.HorizontalSpeed, layer.SortingOrder, layer.Alpha, offset,
                    layer.VerticalRepeatMultiplier, layer.AlignBottomToBaseline);
            }
        }

        private void AddLayer(string name, string resourcePath, float verticalSpeed,
            float horizontalSpeed, int sortingOrder, float alpha, Vector2 offset = default,
            float verticalRepeatMultiplier = 1f, bool alignBottomToBaseline = false)
        {
            var sprite = LoadEnvironmentSprite(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning("Location sprite is missing: " + resourcePath);
                return;
            }

            var layerRoot = new GameObject(name).transform;
            layerRoot.SetParent(_content.transform, false);
            var renderers = new SpriteRenderer[3];
            for (var i = 0; i < renderers.Length; i++)
            {
                var tile = new GameObject(name + " Tile " + i);
                tile.transform.SetParent(layerRoot, false);
                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingLayerName = BackgroundLayer;
                renderer.sortingOrder = sortingOrder;
                renderer.color = new Color(1f, 1f, 1f, alpha);
                renderers[i] = renderer;
            }

            var repeatHeight = sprite.bounds.size.y * Mathf.Max(1f, verticalRepeatMultiplier);
            var tileContentOffsetY = alignBottomToBaseline ? -sprite.bounds.min.y : 0f;
            _layers.Add(new ParallaxLayer(layerRoot, renderers, repeatHeight,
                verticalSpeed, horizontalSpeed, alpha, offset, tileContentOffsetY,
                alignBottomToBaseline));
        }

        private Sprite LoadEnvironmentSprite(string resourcePath)
        {
            var config = _environmentConfig ?? LocationCatalog.Get(_environmentLocation);
            if (config != null)
                return LocationTheme.LoadSprite(config, resourcePath);

            return _environmentLocation == ChallengeLocation.Jungle
                ? Resources.Load<Sprite>("Jungle/" + resourcePath)
                : FutureCityTheme.LoadSprite(resourcePath);
        }

        private void BuildAmbientActors()
        {
            var carSprites = LoadCarSprites();
            var carPairs = new[]
            {
                new DirectionalSprites(carSprites.Car1, carSprites.Car5),
                new DirectionalSprites(carSprites.Car2, carSprites.Car3),
                new DirectionalSprites(carSprites.Car6, carSprites.Car4),
                new DirectionalSprites(carSprites.Car1, carSprites.Car5)
            };
            for (var i = 0; i < 4; i++)
            {
                _actors.Add(CreateCarActor("Car " + (i + 1), carPairs[i],
                    34 + i, FutureCityCarDirection, 0.25f + i * 0.035f, i * 0.4f));
            }

            var birds = FutureCityTheme.LoadSprite("Ambient/birds");
            for (var i = 0; i < 2; i++)
            {
                _actors.Add(CreateActor("Birds " + (i + 1), AmbientKind.Birds,
                    new[] { birds }, 36, i == 0 ? 1f : -1f, 0.16f + i * 0.04f, i * 1.1f));
            }
        }

        private static CarSprites LoadCarSprites()
        {
            // These are directional variants, not frames of one car animation. car_4 and
            // car_6 are currently identical PNGs, but remain separate resources so the
            // directional mapping stays explicit if their artwork later diverges.
            return new CarSprites(
                FutureCityTheme.LoadSprite("Ambient/Cars/car_1"),
                FutureCityTheme.LoadSprite("Ambient/Cars/car_2"),
                FutureCityTheme.LoadSprite("Ambient/Cars/car_3"),
                FutureCityTheme.LoadSprite("Ambient/Cars/car_4"),
                FutureCityTheme.LoadSprite("Ambient/Cars/car_5"),
                FutureCityTheme.LoadSprite("Ambient/Cars/car_6"));
        }

        private AmbientActor CreateCarActor(string name, DirectionalSprites directionalSprites,
            int sortingOrder, float direction, float speed, float phase)
        {
            var actorObject = new GameObject(name);
            actorObject.transform.SetParent(_content.transform, false);
            var renderer = actorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = direction > 0f
                ? directionalSprites.RightSprite
                : directionalSprites.LeftSprite;
            renderer.sortingLayerName = BackgroundLayer;
            renderer.sortingOrder = sortingOrder;

            var laneY = RandomRange(-2.55f, 2.55f);
            var laneX = RandomRange(-1.45f, 1.45f);
            actorObject.transform.position = new Vector3(laneX, laneY, 0f);

            return new AmbientActor(actorObject.transform, renderer, null, directionalSprites,
                AmbientKind.Car, direction, speed, phase, laneY, laneX);
        }

        private AmbientActor CreateActor(string name, AmbientKind kind, Sprite[] frames,
            int sortingOrder, float direction, float speed, float phase)
        {
            var actorObject = new GameObject(name);
            actorObject.transform.SetParent(_content.transform, false);
            var renderer = actorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = frames != null && frames.Length > 0 ? frames[0] : null;
            renderer.sortingLayerName = BackgroundLayer;
            renderer.sortingOrder = sortingOrder;

            var laneY = RandomRange(-2.55f, 2.55f);
            var laneX = RandomRange(-1.45f, 1.45f);
            actorObject.transform.position = new Vector3(laneX, laneY, 0f);

            return new AmbientActor(actorObject.transform, renderer, frames, null, kind, direction,
                speed, phase, laneY, laneX);
        }

        private float RandomRange(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        private void LateUpdate()
        {
            EnsureEnvironmentForCurrentLocation();
            var shouldBeActive = IsEnvironmentActive();
            if (shouldBeActive != _isLocationActive)
                SetLocationActive(shouldBeActive);

            if (!_isLocationActive)
                return;

            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                return;

            UpdateParallax();
            UpdateAmbientActors();
        }

        private void UpdateParallax()
        {
            var cameraY = _camera.transform.position.y;
            var travel = _activationCameraY - cameraY;
            var time = Time.time;

            foreach (var layer in _layers)
                layer.Update(cameraY, travel, time);
        }

        private void UpdateAmbientActors()
        {
            var cameraY = _camera.transform.position.y;
            var time = Time.time;
            var deltaTime = Time.deltaTime;

            foreach (var actor in _actors)
            {
                var position = actor.Transform.position;
                switch (actor.Kind)
                {
                    case AmbientKind.Car:
                        position.x += actor.Direction * actor.Speed * deltaTime;
                        if (actor.Direction > 0f && position.x > HorizontalLimit)
                        {
                            actor.Direction = -1f;
                            actor.Renderer.sprite = actor.DirectionalSprites.LeftSprite;
                        }
                        else if (actor.Direction < 0f && position.x < -HorizontalLimit)
                        {
                            actor.Direction = 1f;
                            actor.Renderer.sprite = actor.DirectionalSprites.RightSprite;
                        }

                        position.y = cameraY + actor.LaneY +
                                     Mathf.Sin(time * 1.15f + actor.Phase) * 0.035f;
                        actor.Renderer.flipX = false;
                        break;
                    case AmbientKind.Birds:
                        position.x += actor.Direction * actor.Speed * deltaTime;
                        if (position.x > HorizontalLimit)
                            position.x = -HorizontalLimit;
                        else if (position.x < -HorizontalLimit)
                            position.x = HorizontalLimit;

                        position.y = cameraY + actor.LaneY +
                                     Mathf.Sin(time * 1.15f + actor.Phase) * 0.035f;
                        actor.Renderer.flipX = actor.Direction > 0f;
                        break;
                }

                actor.Transform.position = position;
                if (actor.Kind == AmbientKind.Birds && actor.Frames != null && actor.Frames.Length > 1)
                {
                    var frame = Mathf.FloorToInt((time + actor.Phase) * 8f) % actor.Frames.Length;
                    actor.Renderer.sprite = actor.Frames[frame];
                }
            }
        }

        private void SetLocationActive(bool isActive)
        {
            if (_content == null)
                return;

            _isLocationActive = isActive;
            if (isActive && _camera != null)
                _activationCameraY = _camera.transform.position.y;

            _content.SetActive(isActive);
            CacheSceneThemeEntries();
            CacheWallLightnings();
            ChallengeLocation? location = isActive ? _environmentLocation : null;
            SetSceneTheme(location);
            ApplyWallLightningOffsets(location);
            ConfigureStartArea(location);
        }

        private void SetSceneTheme(ChallengeLocation? location)
        {
            foreach (var entry in _themeEntries)
                entry.SetTheme(location);
        }

        private ChallengeLocation GetEnvironmentLocation()
        {
            if (_gameController == null || _gameController.Mode != GameMode.Challenge ||
                _gameController.CurrentChallengeDefinition == null)
                return ChallengeLocation.DeepBunker;

            return _gameController.CurrentChallengeDefinition.Location;
        }

        private bool IsEnvironmentActive()
        {
            if (_gameController == null || _gameController.Mode != GameMode.Challenge)
                return false;

            var config = _environmentConfig ?? LocationCatalog.Get(_environmentLocation);
            if (config == null || config.EnvironmentLayers == null ||
                config.EnvironmentLayers.Length == 0)
                return false;

            return GetEnvironmentLocation() == _environmentLocation;
        }

        private void CacheSceneThemeEntries()
        {
            var renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer == null || !renderer.gameObject.scene.IsValid() ||
                    renderer.transform.IsChildOf(transform) || !_themedRendererIds.Add(renderer.GetEntityId()))
                    continue;

                var textureName = renderer.sprite != null && renderer.sprite.texture != null
                    ? renderer.sprite.texture.name
                    : string.Empty;
                if (IsThemedTexture(textureName))
                    _themeEntries.Add(new SceneThemeEntry(renderer, textureName));
            }
        }

        private void CacheWallLightnings()
        {
            foreach (var transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (transform == null || !transform.gameObject.scene.IsValid())
                    continue;

                var isLeftWall = transform.name == "WallL1" || transform.name == "WallL2";
                var isRightWall = transform.name == "WallR1" || transform.name == "WallR2";
                if (!isLeftWall && !isRightWall)
                    continue;

                foreach (var child in transform.GetComponentsInChildren<Transform>(true))
                {
                    if (child == transform || !child.name.StartsWith("Lightning", StringComparison.Ordinal) ||
                        !_wallLightningTransforms.Add(child))
                        continue;

                    _wallLightnings.Add(new WallLightning(child, isLeftWall));
                }
            }
        }

        private void ApplyWallLightningOffsets(ChallengeLocation? location)
        {
            var config = location.HasValue ? LocationCatalog.Get(location.Value) : null;
            var visuals = config != null ? config.StartArea : null;
            foreach (var lightning in _wallLightnings)
            {
                lightning.SetLocalX(visuals == null || !visuals.OverrideWallLightningOffsets
                    ? lightning.OriginalLocalPosition.x
                    : lightning.IsLeft
                        ? visuals.LeftWallLightningLocalX
                        : visuals.RightWallLightningLocalX);
            }
        }

        private void ConfigureStartArea(ChallengeLocation? location)
        {
            var wallController = FindAnyObjectByType<WallController>();
            if (wallController == null)
                return;

            var config = location.HasValue ? LocationCatalog.Get(location.Value) : null;
            var visuals = config != null ? config.StartArea : null;
            wallController.ConfigureStartArea(
                visuals != null ? visuals.StartPlatformOffset : Vector2.zero,
                visuals == null || visuals.StartWallsAreOut);
        }

        private static bool IsThemedTexture(string textureName)
        {
            switch (textureName)
            {
                case "bg_blue":
                case "fg_wall_l":
                case "fg_wall_r":
                case "spr_start_platform":
                case "start_door":
                case "door_l":
                case "door_r":
                case "vfx_discharge_wall":
                    return true;
                default:
                    return false;
            }
        }

        private enum AmbientKind
        {
            Car,
            Birds
        }

        private sealed class WallLightning
        {
            private readonly Transform _transform;
            public readonly bool IsLeft;
            public readonly Vector3 OriginalLocalPosition;

            public WallLightning(Transform transform, bool isLeft)
            {
                _transform = transform;
                IsLeft = isLeft;
                OriginalLocalPosition = transform.localPosition;
            }

            public void SetLocalX(float x)
            {
                if (_transform == null)
                    return;

                var position = _transform.localPosition;
                position.x = x;
                _transform.localPosition = position;
            }
        }

        private sealed class ParallaxLayer
        {
            private readonly Transform _root;
            private readonly SpriteRenderer[] _renderers;
            private readonly float _height;
            private readonly float _verticalSpeed;
            private readonly float _horizontalSpeed;
            private readonly float _alpha;
            private readonly Vector2 _offset;
            private readonly float _tileContentOffsetY;
            private readonly bool _usesTravelParallax;

            public ParallaxLayer(Transform root, SpriteRenderer[] renderers, float height,
                float verticalSpeed, float horizontalSpeed, float alpha, Vector2 offset,
                float tileContentOffsetY, bool usesTravelParallax)
            {
                _root = root;
                _renderers = renderers;
                _height = height;
                _verticalSpeed = verticalSpeed;
                _horizontalSpeed = horizontalSpeed;
                _alpha = alpha;
                _offset = offset;
                _tileContentOffsetY = tileContentOffsetY;
                _usesTravelParallax = usesTravelParallax;
            }

            public void Update(float cameraY, float travel, float time)
            {
                var offset = Mathf.Repeat(travel * _verticalSpeed + _height * 0.5f, _height) -
                             _height * 0.5f;
                var x = _horizontalSpeed == 0f
                    ? 0f
                    : Mathf.Sin((_usesTravelParallax ? travel : time) * Mathf.Abs(_horizontalSpeed)) *
                      (_usesTravelParallax ? 0.12f : 0.055f) *
                      Mathf.Sign(_horizontalSpeed);
                _root.position = new Vector3(_offset.x + x, cameraY + _offset.y + offset, 0f);

                for (var i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].transform.localPosition = new Vector3(
                        0f,
                        (i - 1) * _height + _tileContentOffsetY,
                        0f);
                    var pulse = 0.96f + Mathf.Sin(time * 0.45f + i) * 0.04f;
                    _renderers[i].color = new Color(1f, 1f, 1f, _alpha * pulse);
                }
            }
        }

        private sealed class AmbientActor
        {
            public readonly Transform Transform;
            public readonly SpriteRenderer Renderer;
            public readonly Sprite[] Frames;
            public readonly DirectionalSprites DirectionalSprites;
            public readonly AmbientKind Kind;
            public float Direction;
            public readonly float Speed;
            public readonly float Phase;
            public readonly float LaneY;
            public readonly float LaneX;

            public AmbientActor(Transform transform, SpriteRenderer renderer, Sprite[] frames,
                DirectionalSprites directionalSprites, AmbientKind kind, float direction, float speed,
                float phase, float laneY, float laneX)
            {
                Transform = transform;
                Renderer = renderer;
                Frames = frames;
                DirectionalSprites = directionalSprites;
                Kind = kind;
                Direction = direction;
                Speed = speed;
                Phase = phase;
                LaneY = laneY;
                LaneX = laneX;
            }
        }

        private sealed class DirectionalSprites
        {
            public readonly Sprite RightSprite;
            public readonly Sprite LeftSprite;

            public DirectionalSprites(Sprite rightSprite, Sprite leftSprite)
            {
                RightSprite = rightSprite;
                LeftSprite = leftSprite;
            }
        }

        private readonly struct CarSprites
        {
            public readonly Sprite Car1;
            public readonly Sprite Car2;
            public readonly Sprite Car3;
            public readonly Sprite Car4;
            public readonly Sprite Car5;
            public readonly Sprite Car6;

            public CarSprites(Sprite car1, Sprite car2, Sprite car3, Sprite car4,
                Sprite car5, Sprite car6)
            {
                Car1 = car1;
                Car2 = car2;
                Car3 = car3;
                Car4 = car4;
                Car5 = car5;
                Car6 = car6;
            }
        }

        private sealed class SceneThemeEntry
        {
            private readonly SpriteRenderer _renderer;
            private readonly Sprite _originalSprite;
            private readonly bool _originalEnabled;
            private readonly string _originalTextureName;
            private readonly Vector3 _originalLocalPosition;
            private readonly Animator _animator;
            private readonly bool _animatorWasEnabled;
            private FutureCityFrameAnimator _frameAnimator;

            public SceneThemeEntry(SpriteRenderer renderer, string originalTextureName)
            {
                _renderer = renderer;
                _originalSprite = renderer.sprite;
                _originalEnabled = renderer.enabled;
                _originalTextureName = originalTextureName;
                _originalLocalPosition = renderer.transform.localPosition;
                _animator = renderer.GetComponent<Animator>();
                _animatorWasEnabled = _animator != null && _animator.enabled;
            }

            public void SetTheme(ChallengeLocation? location)
            {
                if (_renderer == null)
                    return;

                var visual = GetVisual(location, _originalTextureName);
                if (!visual.IsThemed)
                {
                    if (_frameAnimator != null)
                        _frameAnimator.Stop();
                    if (_animator != null)
                        _animator.enabled = _animatorWasEnabled;
                    _renderer.enabled = _originalEnabled;
                    _renderer.sprite = _originalSprite;
                    _renderer.transform.localPosition = _originalLocalPosition;
                    return;
                }

                _renderer.enabled = !visual.Hide && _originalEnabled;
                if (visual.Sprite != null)
                    _renderer.sprite = visual.Sprite;
                _renderer.transform.localPosition = _originalLocalPosition +
                    new Vector3(visual.LocalPositionOffset.x, visual.LocalPositionOffset.y, 0f);

                if (visual.AnimationFrames == null || visual.AnimationFrames.Length == 0)
                {
                    if (_frameAnimator != null)
                        _frameAnimator.Stop();
                    if (_animator != null)
                        _animator.enabled = _animatorWasEnabled;
                    return;
                }

                if (_animator != null)
                    _animator.enabled = false;
                if (_frameAnimator == null)
                    _frameAnimator = _renderer.gameObject.AddComponent<FutureCityFrameAnimator>();
                _frameAnimator.Play(_renderer, visual.AnimationFrames, visual.FramesPerSecond,
                    Mathf.Abs(_renderer.GetEntityId().GetHashCode() % 10) * 0.03f);
            }

            private static LocationVisual GetVisual(ChallengeLocation? location, string textureName)
            {
                var config = location.HasValue ? LocationCatalog.Get(location.Value) : null;
                if (config == null)
                    return default;

                var visuals = config.StartArea;
                if (visuals == null)
                    return default;
                switch (textureName)
                {
                    case "bg_blue": return visuals.HideBaseBackground ? LocationVisual.Hidden : default;
                    case "fg_wall_l": return LocationVisual.SpriteOnly(LocationTheme.LoadSprite(config, visuals.LeftWallPath));
                    case "fg_wall_r": return LocationVisual.SpriteOnly(LocationTheme.LoadSprite(config, visuals.RightWallPath));
                    case "spr_start_platform": return LocationVisual.SpriteOnly(
                        LocationTheme.LoadSprite(config, visuals.StartPlatformPath), visuals.StartPlatformOffset);
                    case "start_door": return LocationVisual.SpriteOnly(LocationTheme.LoadSprite(config, visuals.StartDoorFramePath));
                    case "door_l": return LocationVisual.SpriteOnly(LocationTheme.LoadSprite(config, visuals.LeftDoorPath));
                    case "door_r": return LocationVisual.SpriteOnly(LocationTheme.LoadSprite(config, visuals.RightDoorPath));
                    case "vfx_discharge_wall": return LocationVisual.Animated(
                        LocationTheme.LoadFrames(config, visuals.WallVfxPath), visuals.WallVfxFramesPerSecond);
                    default: return default;
                }
            }

            private readonly struct LocationVisual
            {
                public static readonly LocationVisual Hidden = new LocationVisual(true, true, null, null, 0f);

                public readonly bool IsThemed;
                public readonly bool Hide;
                public readonly Sprite Sprite;
                public readonly Sprite[] AnimationFrames;
                public readonly float FramesPerSecond;
                public readonly Vector2 LocalPositionOffset;

                private LocationVisual(bool isThemed, bool hide, Sprite sprite, Sprite[] animationFrames,
                    float framesPerSecond, Vector2 localPositionOffset = default)
                {
                    IsThemed = isThemed;
                    Hide = hide;
                    Sprite = sprite;
                    AnimationFrames = animationFrames;
                    FramesPerSecond = framesPerSecond;
                    LocalPositionOffset = localPositionOffset;
                }

                public static LocationVisual SpriteOnly(Sprite sprite, Vector2 localPositionOffset = default)
                {
                    return new LocationVisual(sprite != null, false, sprite, null, 0f,
                        localPositionOffset);
                }

                public static LocationVisual Animated(Sprite[] frames, float framesPerSecond)
                {
                    return new LocationVisual(frames != null && frames.Length > 0, false, null,
                        frames, framesPerSecond);
                }
            }
        }
    }
}
