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

        private readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();
        private readonly List<AmbientActor> _actors = new List<AmbientActor>();
        private readonly List<SceneThemeEntry> _themeEntries = new List<SceneThemeEntry>();
        private readonly HashSet<EntityId> _themedRendererIds = new HashSet<EntityId>();

        private GameController _gameController;
        private Camera _camera;
        private GameObject _content;
        private bool _isLocationActive;
        private float _activationCameraY;
        private System.Random _random;
        private Sprite[] _carFrames;
        private ChallengeLocation _environmentLocation;

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

            SetSceneTheme(ChallengeLocation.DeepBunker);
            _isLocationActive = false;
            if (_content != null)
            {
                _content.SetActive(false);
                Destroy(_content);
                _content = null;
            }

            _layers.Clear();
            _actors.Clear();
            BuildEnvironment(location);
        }

        private void BuildEnvironment(ChallengeLocation location)
        {
            _environmentLocation = location;
            _content = new GameObject("Content");
            _content.transform.SetParent(transform, false);

            if (location == ChallengeLocation.FutureCity)
                BuildFutureCityEnvironment();
            else if (location == ChallengeLocation.Jungle)
                BuildJungleEnvironment();

            _content.SetActive(false);
        }

        private void BuildFutureCityEnvironment()
        {
            AddLayer("Sky", "Environment/sky", 0.015f, 0f, 0, 1f);
            AddLayer("Far City", "Environment/city_4", 0.055f, 0f, 10, 1f);
            AddLayer("Mid City A", "Environment/city_3", 0.10f, 0f, 20, 1f);
            AddLayer("Far Clouds", "Environment/clouds_far", 0.13f, 0.27f, 24, 0.72f);
            AddLayer("Mid City B", "Environment/city_2", 0.17f, 0f, 30, 1f);
            AddLayer("Near City", "Environment/city_1", 0.24f, 0f, 40, 1f);
            AddLayer("Near Clouds", "Environment/clouds_near", 0.29f, -0.20f, 44, 0.36f);

            BuildAmbientActors();
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

                AddLayer(layer.Name, layer.ResourcePath, layer.VerticalSpeed,
                    layer.HorizontalSpeed, layer.SortingOrder, layer.Alpha, layer.Offset);
            }
        }

        private void AddLayer(string name, string resourcePath, float verticalSpeed,
            float horizontalSpeed, int sortingOrder, float alpha, Vector2 offset = default)
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

            _layers.Add(new ParallaxLayer(layerRoot, renderers, sprite.bounds.size.y,
                verticalSpeed, horizontalSpeed, alpha, offset));
        }

        private Sprite LoadEnvironmentSprite(string resourcePath)
        {
            return _environmentLocation == ChallengeLocation.Jungle
                ? Resources.Load<Sprite>("Jungle/" + resourcePath)
                : FutureCityTheme.LoadSprite(resourcePath);
        }

        private void BuildAmbientActors()
        {
            _carFrames = FutureCityTheme.LoadFrames("Ambient/Cars");
            for (var i = 0; i < 4; i++)
            {
                var direction = (i % 2 == 0) ? -1f : 1f;
                _actors.Add(CreateActor("Car " + (i + 1), AmbientKind.Car, _carFrames,
                    34 + i, direction, 0.25f + i * 0.035f, i * 0.4f));
            }

            var birds = FutureCityTheme.LoadSprite("Ambient/birds");
            for (var i = 0; i < 2; i++)
            {
                _actors.Add(CreateActor("Birds " + (i + 1), AmbientKind.Birds,
                    new[] { birds }, 36, i == 0 ? 1f : -1f, 0.16f + i * 0.04f, i * 1.1f));
            }

            var smoke = FutureCityTheme.LoadSprite("Ambient/smoke");
            for (var i = 0; i < 3; i++)
            {
                _actors.Add(CreateActor("Smoke " + (i + 1), AmbientKind.Smoke,
                    new[] { smoke }, 42, 0f, 0.08f + i * 0.015f, i * 1.7f));
            }
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

            return new AmbientActor(actorObject.transform, renderer, frames, kind, direction,
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
                    case AmbientKind.Smoke:
                        position.x = actor.LaneX + Mathf.Sin(time * 0.55f + actor.Phase) * 0.06f;
                        position.y = cameraY - 2.8f +
                                     Mathf.Repeat(time * actor.Speed + actor.Phase, 5.6f);
                        var height01 = Mathf.InverseLerp(cameraY - 2.8f, cameraY + 2.8f, position.y);
                        actor.Renderer.color = new Color(1f, 1f, 1f,
                            Mathf.Sin(height01 * Mathf.PI) * 0.48f);
                        break;
                }

                actor.Transform.position = position;
                if (actor.Frames != null && actor.Frames.Length > 1)
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
            SetSceneTheme(isActive ? _environmentLocation : ChallengeLocation.DeepBunker);
        }

        private void SetSceneTheme(ChallengeLocation location)
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
            if (_environmentLocation != ChallengeLocation.FutureCity &&
                _environmentLocation != ChallengeLocation.Jungle)
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
            Birds,
            Smoke
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

            public ParallaxLayer(Transform root, SpriteRenderer[] renderers, float height,
                float verticalSpeed, float horizontalSpeed, float alpha, Vector2 offset)
            {
                _root = root;
                _renderers = renderers;
                _height = height;
                _verticalSpeed = verticalSpeed;
                _horizontalSpeed = horizontalSpeed;
                _alpha = alpha;
                _offset = offset;
            }

            public void Update(float cameraY, float travel, float time)
            {
                var offset = Mathf.Repeat(travel * _verticalSpeed + _height * 0.5f, _height) -
                             _height * 0.5f;
                var x = _horizontalSpeed == 0f
                    ? 0f
                    : Mathf.Sin(time * Mathf.Abs(_horizontalSpeed)) * 0.055f * Mathf.Sign(_horizontalSpeed);
                _root.position = new Vector3(_offset.x + x, cameraY + _offset.y + offset, 0f);

                for (var i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].transform.localPosition = new Vector3(0f, (i - 1) * _height, 0f);
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
            public readonly AmbientKind Kind;
            public readonly float Direction;
            public readonly float Speed;
            public readonly float Phase;
            public readonly float LaneY;
            public readonly float LaneX;

            public AmbientActor(Transform transform, SpriteRenderer renderer, Sprite[] frames,
                AmbientKind kind, float direction, float speed, float phase, float laneY, float laneX)
            {
                Transform = transform;
                Renderer = renderer;
                Frames = frames;
                Kind = kind;
                Direction = direction;
                Speed = speed;
                Phase = phase;
                LaneY = laneY;
                LaneX = laneX;
            }
        }

        private sealed class SceneThemeEntry
        {
            private readonly SpriteRenderer _renderer;
            private readonly Sprite _originalSprite;
            private readonly bool _originalEnabled;
            private readonly string _originalTextureName;
            private readonly Animator _animator;
            private readonly bool _animatorWasEnabled;
            private FutureCityFrameAnimator _frameAnimator;

            public SceneThemeEntry(SpriteRenderer renderer, string originalTextureName)
            {
                _renderer = renderer;
                _originalSprite = renderer.sprite;
                _originalEnabled = renderer.enabled;
                _originalTextureName = originalTextureName;
                _animator = renderer.GetComponent<Animator>();
                _animatorWasEnabled = _animator != null && _animator.enabled;
            }

            public void SetTheme(ChallengeLocation location)
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
                    return;
                }

                _renderer.enabled = !visual.Hide && _originalEnabled;
                if (visual.Sprite != null)
                    _renderer.sprite = visual.Sprite;

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

            private static LocationVisual GetVisual(ChallengeLocation location, string textureName)
            {
                if (location == ChallengeLocation.FutureCity)
                    return GetFutureCityVisual(textureName);
                if (location == ChallengeLocation.Jungle)
                    return GetJungleVisual(textureName);
                return default;
            }

            private static LocationVisual GetFutureCityVisual(string textureName)
            {
                switch (textureName)
                {
                    case "bg_blue": return LocationVisual.Hidden;
                    case "fg_wall_l": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/wall_left"));
                    case "fg_wall_r": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/wall_right"));
                    case "spr_start_platform": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/start_platform"));
                    case "start_door": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/start_door"));
                    case "door_l": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/door_left"));
                    case "door_r": return LocationVisual.SpriteOnly(FutureCityTheme.LoadSprite("Start/door_right"));
                    case "vfx_discharge_wall": return LocationVisual.Animated(FutureCityTheme.LoadFrames("VFX/Wall"), 10f);
                    default: return default;
                }
            }

            private static LocationVisual GetJungleVisual(string textureName)
            {
                var config = JungleTheme.Config;
                if (config == null)
                    return default;

                var visuals = config.Visuals;
                switch (textureName)
                {
                    case "bg_blue": return LocationVisual.Hidden;
                    case "fg_wall_l": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.LeftWallPath));
                    case "fg_wall_r": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.RightWallPath));
                    case "spr_start_platform": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.StartPlatformPath));
                    case "start_door": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.StartDoorFramePath));
                    case "door_l": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.LeftDoorPath));
                    case "door_r": return LocationVisual.SpriteOnly(JungleTheme.LoadSprite(visuals.RightDoorPath));
                    case "vfx_discharge_wall": return LocationVisual.Animated(
                        JungleTheme.LoadFrames(visuals.WallVfxPath), 10f);
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

                private LocationVisual(bool isThemed, bool hide, Sprite sprite, Sprite[] animationFrames,
                    float framesPerSecond)
                {
                    IsThemed = isThemed;
                    Hide = hide;
                    Sprite = sprite;
                    AnimationFrames = animationFrames;
                    FramesPerSecond = framesPerSecond;
                }

                public static LocationVisual SpriteOnly(Sprite sprite)
                {
                    return new LocationVisual(sprite != null, false, sprite, null, 0f);
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
