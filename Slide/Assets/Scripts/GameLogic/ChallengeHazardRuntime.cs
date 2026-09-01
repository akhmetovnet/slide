using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    // Runtime-only extension of the existing pooled Thorn prefab. All colliders and
    // visual children are created once per pooled thorn, then reset and reused.
    public sealed class ChallengeHazardRuntime : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite[]> FrameCache =
            new Dictionary<string, Sprite[]>();

        private SpriteRenderer _renderer;
        private Rigidbody2D _rigidbody;
        private BoxCollider2D _areaCollider;
        private BoxCollider2D _barrierCollider;
        private FutureCityFrameAnimator _frameAnimator;
        private CircleCollider2D[] _spikeColliders = Array.Empty<CircleCollider2D>();
        private Transform _barrierVisualRoot;
        private SpriteRenderer _barrierLeftRenderer;
        private SpriteRenderer _barrierRightRenderer;
        private SpriteRenderer _barrierVfxRenderer;
        private FutureCityFrameAnimator _barrierFrameAnimator;

        private ThornType _type = ThornType.None;
        private LocationHazardSettings _settings;
        private LocationConfig _config;
        private float _speedMultiplier;
        private float _elapsed;
        private float _stickyMovementMultiplier = 1f;
        private Quaternion _startRotation;

        public bool IsSticky => gameObject.activeInHierarchy && _type == ThornType.StickySurface;
        public float StickyMovementMultiplier => _stickyMovementMultiplier;

        public void Initialize(SpriteRenderer referenceRenderer)
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            if (referenceRenderer != null)
            {
                _renderer.sortingLayerID = referenceRenderer.sortingLayerID;
                _renderer.sortingOrder = referenceRenderer.sortingOrder + 1;
            }

            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.simulated = true;

            _areaCollider = gameObject.AddComponent<BoxCollider2D>();
            _areaCollider.isTrigger = true;
            _areaCollider.enabled = false;
            _barrierCollider = gameObject.AddComponent<BoxCollider2D>();
            _barrierCollider.isTrigger = true;
            _barrierCollider.enabled = false;
            gameObject.SetActive(false);
        }

        public void Configure(ThornType type, float speedMultiplier)
        {
            Configure(type, speedMultiplier, LocationCatalog.Get(ChallengeLocation.Jungle));
        }

        public void Configure(ThornType type, float speedMultiplier, JungleHazardSettings settings)
        {
            Configure(type, speedMultiplier, LocationHazardSettings.FromLegacy(settings),
                LocationCatalog.Get(ChallengeLocation.Jungle));
        }

        public void Configure(ThornType type, float speedMultiplier, LocationConfig config)
        {
            Configure(type, speedMultiplier, config != null ? config.Hazards : null, config);
        }

        private void Configure(ThornType type, float speedMultiplier,
            LocationHazardSettings settings, LocationConfig config)
        {
            if (settings == null)
            {
                Clear();
                return;
            }

            ResetState(false);
            _type = type;
            _settings = settings;
            _config = config;
            _speedMultiplier = Mathf.Max(0.01f, speedMultiplier);
            _elapsed = 0f;
            _startRotation = Quaternion.Euler(0f, 0f, GetStartAngle(type));
            transform.localPosition = Vector3.zero;
            transform.localRotation = _startRotation;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);

            switch (type)
            {
                case ThornType.RotatingSpikes:
                    ConfigureRotatingSpikes();
                    break;
                case ThornType.PopUpSpikes:
                    ConfigurePopUpSpikes();
                    break;
                case ThornType.StickySurface:
                    ConfigureStickySurface();
                    break;
                case ThornType.RotatingLaser:
                    ConfigureRotatingLaser();
                    break;
            }
        }

        private float GetStartAngle(ThornType type)
        {
            return type == ThornType.RotatingSpikes
                ? _settings.RotatingSpikesStartAngle
                : type == ThornType.RotatingLaser ? _settings.RotatingBarrierStartAngle : 0f;
        }

        private void ConfigureRotatingSpikes()
        {
            PlayFrames(_settings.RotatingSpikesVisualPath, 12f * _speedMultiplier);
            transform.localScale = Vector3.one * _settings.RotatingSpikesScale;
            EnsureSpikeColliders(_settings.RotatingSpikesSectionCount,
                _settings.RotatingSpikesSectionDistance, _settings.RotatingSpikesColliderRadius);
            gameObject.tag = "Untagged";
        }

        private void ConfigurePopUpSpikes()
        {
            _renderer.sprite = LoadSprite(_settings.PopUpSpikesVisualPath);
            transform.localPosition = new Vector3(0f, _settings.PopUpHiddenHeight, 0f);
            transform.localScale = new Vector3(1.8f, 0.8f, 1f);
            _areaCollider.size = _settings.PopUpColliderSize;
            _areaCollider.enabled = false;
            gameObject.tag = "Thorn";
        }

        private void ConfigureStickySurface()
        {
            _renderer.sprite = LoadSprite(_settings.StickyVisualPath);
            _renderer.color = new Color(0.55f, 1f, 0.25f, 0.9f);
            transform.localScale = new Vector3(1.45f, 0.55f, 1f);
            transform.localPosition = new Vector3(0f, 0.88f, 0f);
            _areaCollider.size = _settings.StickyColliderSize;
            _areaCollider.enabled = true;
            _stickyMovementMultiplier = _settings.StickyMovementMultiplier;
            gameObject.tag = "Untagged";
        }

        private void ConfigureRotatingLaser()
        {
            if (_config == null || _config.HazardVisuals == null)
            {
                Clear();
                return;
            }

            EnsureBarrierVisuals();
            var visuals = _config.HazardVisuals;
            _barrierLeftRenderer.sprite = LocationTheme.LoadSprite(_config, visuals.BarrierLeftPath);
            _barrierRightRenderer.sprite = LocationTheme.LoadSprite(_config, visuals.BarrierRightPath);
            var frames = LocationTheme.LoadFrames(_config, visuals.BarrierVfxPath);
            _renderer.sprite = frames.Length > 0 ? frames[0] : null;
            _renderer.enabled = false;
            _barrierFrameAnimator.Play(_barrierVfxRenderer, frames, 10f);

            var vfxWidth = frames.Length > 0 ? frames[0].bounds.size.x : 0f;
            _barrierVfxRenderer.transform.localScale = new Vector3(
                vfxWidth > 0f ? _settings.RotatingBarrierLength / vfxWidth : 1f, 1f, 1f);

            var halfLength = _settings.RotatingBarrierLength * 0.5f;
            _barrierLeftRenderer.transform.localPosition = new Vector3(-halfLength, 0f, 0f);
            _barrierRightRenderer.transform.localPosition = new Vector3(halfLength, 0f, 0f);
            _barrierCollider.size = new Vector2(_settings.RotatingBarrierLength,
                _settings.RotatingBarrierThickness);
            _barrierCollider.enabled = true;
            _barrierVisualRoot.gameObject.SetActive(true);
            gameObject.tag = "Thorn";
        }

        private void EnsureSpikeColliders(int count, float distance, float radius)
        {
            count = Mathf.Max(1, count);
            if (_spikeColliders.Length < count)
            {
                var colliders = new CircleCollider2D[count];
                for (var i = 0; i < count; i++)
                {
                    if (i < _spikeColliders.Length)
                    {
                        colliders[i] = _spikeColliders[i];
                        continue;
                    }

                    var section = new GameObject("Spike damage section " + (i + 1));
                    section.transform.SetParent(transform, false);
                    section.tag = "Thorn";
                    var collider = section.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true;
                    colliders[i] = collider;
                }

                _spikeColliders = colliders;
            }

            for (var i = 0; i < _spikeColliders.Length; i++)
            {
                var collider = _spikeColliders[i];
                var isActive = i < count;
                collider.enabled = isActive;
                if (!isActive)
                    continue;

                var angle = i * Mathf.PI * 2f / count;
                collider.transform.localPosition = new Vector3(Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance, 0f);
                collider.radius = radius;
            }
        }

        private void EnsureBarrierVisuals()
        {
            if (_barrierVisualRoot != null)
                return;

            _barrierVisualRoot = new GameObject("Rotating barrier visual").transform;
            _barrierVisualRoot.SetParent(transform, false);
            _barrierLeftRenderer = CreateBarrierRenderer("Left", 0);
            _barrierRightRenderer = CreateBarrierRenderer("Right", 0);
            _barrierVfxRenderer = CreateBarrierRenderer("Discharge", 1);
            _barrierFrameAnimator = _barrierVfxRenderer.gameObject.AddComponent<FutureCityFrameAnimator>();
        }

        private SpriteRenderer CreateBarrierRenderer(string name, int sortingOrderOffset)
        {
            var visual = new GameObject(name);
            visual.transform.SetParent(_barrierVisualRoot, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingLayerID = _renderer.sortingLayerID;
            renderer.sortingOrder = _renderer.sortingOrder + sortingOrderOffset;
            return renderer;
        }

        private void PlayFrames(string resourcePath, float framesPerSecond)
        {
            var frames = LoadFrames(resourcePath);
            if (frames.Length == 0)
            {
                _renderer.sprite = LoadSprite(resourcePath);
                return;
            }

            if (_frameAnimator == null)
                _frameAnimator = gameObject.AddComponent<FutureCityFrameAnimator>();
            _frameAnimator.Play(_renderer, frames, framesPerSecond);
        }

        private Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return null;
            return _config != null
                ? LocationTheme.LoadSprite(_config, resourcePath)
                : Resources.Load<Sprite>(resourcePath);
        }

        private Sprite[] LoadFrames(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return Array.Empty<Sprite>();

            if (_config != null)
                return LocationTheme.LoadFrames(_config, resourcePath);

            if (!FrameCache.TryGetValue(resourcePath, out var frames))
            {
                frames = Resources.LoadAll<Sprite>(resourcePath);
                System.Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
                FrameCache[resourcePath] = frames;
            }

            return frames;
        }

        private void Update()
        {
            if (_type == ThornType.None)
                return;

            _elapsed += Time.deltaTime;
            switch (_type)
            {
                case ThornType.RotatingSpikes:
                    transform.localRotation = _startRotation * Quaternion.Euler(0f, 0f,
                        _settings.RotatingSpikesDegreesPerSecond * _speedMultiplier * _elapsed);
                    break;
                case ThornType.PopUpSpikes:
                    UpdatePopUpSpikes();
                    break;
                case ThornType.StickySurface:
                    UpdateStickySurface();
                    break;
                case ThornType.RotatingLaser:
                    UpdateRotatingLaser();
                    break;
            }
        }

        private void UpdatePopUpSpikes()
        {
            var warning = _settings.PopUpWarningTime / _speedMultiplier;
            var extend = _settings.PopUpExtendTime / _speedMultiplier;
            var active = _settings.PopUpActiveTime / _speedMultiplier;
            var retract = _settings.PopUpRetractTime / _speedMultiplier;
            var cooldown = _settings.PopUpCooldownTime / _speedMultiplier;
            var cycle = warning + extend + active + retract + cooldown;
            var time = Mathf.Repeat(_elapsed, cycle);
            var height = _settings.PopUpHiddenHeight;
            var isDangerous = false;

            if (time < warning)
            {
                _renderer.color = Color.yellow;
            }
            else if (time < warning + extend)
            {
                var t = (time - warning) / extend;
                height = Mathf.Lerp(_settings.PopUpHiddenHeight, _settings.PopUpActiveHeight, t);
                _renderer.color = Color.white;
            }
            else if (time < warning + extend + active)
            {
                height = _settings.PopUpActiveHeight;
                _renderer.color = Color.white;
                isDangerous = true;
            }
            else if (time < warning + extend + active + retract)
            {
                var t = (time - warning - extend - active) / retract;
                height = Mathf.Lerp(_settings.PopUpActiveHeight, _settings.PopUpHiddenHeight, t);
                _renderer.color = Color.white;
            }
            else
            {
                _renderer.color = Color.white;
            }

            transform.localPosition = new Vector3(0f, height, 0f);
            _areaCollider.enabled = isDangerous;
        }

        private void UpdateStickySurface()
        {
            var scale = transform.localScale;
            scale.y = 0.55f + Mathf.Sin(_elapsed * 3.6f) * 0.08f;
            transform.localScale = scale;
        }

        private void UpdateRotatingLaser()
        {
            transform.localRotation = _startRotation * Quaternion.Euler(0f, 0f,
                _settings.RotatingBarrierDegreesPerSecond * _speedMultiplier * _elapsed);
            if (_settings.RotatingBarrierIsContinuous)
                return;

            var activeTime = _settings.RotatingBarrierActiveTime / _speedMultiplier;
            var inactiveTime = _settings.RotatingBarrierInactiveTime / _speedMultiplier;
            var isActive = Mathf.Repeat(_elapsed, activeTime + inactiveTime) < activeTime;
            _barrierCollider.enabled = isActive;
            _barrierVisualRoot.gameObject.SetActive(isActive);
        }

        public void Clear()
        {
            ResetState(true);
        }

        private void ResetState(bool deactivate)
        {
            _frameAnimator?.Stop();
            _barrierFrameAnimator?.Stop();
            _type = ThornType.None;
            _settings = null;
            _config = null;
            _elapsed = 0f;
            _stickyMovementMultiplier = 1f;
            if (_areaCollider != null)
                _areaCollider.enabled = false;
            if (_barrierCollider != null)
                _barrierCollider.enabled = false;
            foreach (var collider in _spikeColliders)
            {
                if (collider != null)
                    collider.enabled = false;
            }

            if (_barrierVisualRoot != null)
                _barrierVisualRoot.gameObject.SetActive(false);
            if (_renderer != null)
            {
                _renderer.enabled = true;
                _renderer.sprite = null;
                _renderer.color = Color.white;
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            gameObject.tag = "Untagged";
            if (deactivate)
                gameObject.SetActive(false);
        }
    }
}
