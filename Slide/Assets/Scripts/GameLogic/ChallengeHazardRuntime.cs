using DG.Tweening;
using UnityEngine;

namespace GameLogic
{
    public sealed class ChallengeHazardRuntime : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private Rigidbody2D _rigidbody;
        private FutureCityFrameAnimator _frameAnimator;
        private Tween _motionTween;
        private ThornType _type = ThornType.None;
        private float _rotationSpeed;

        public bool IsSticky => gameObject.activeInHierarchy && _type == ThornType.StickySurface;

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
            gameObject.SetActive(false);
        }

        public void Configure(ThornType type, float speedMultiplier)
        {
            Clear();
            _type = type;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);

            switch (type)
            {
                case ThornType.RotatingSpikes:
                    ConfigureRotatingSpikes(speedMultiplier);
                    break;
                case ThornType.PopUpSpikes:
                    ConfigurePopUpSpikes(speedMultiplier);
                    break;
                case ThornType.StickySurface:
                    ConfigureStickySurface();
                    break;
                case ThornType.RotatingLaser:
                    ConfigureRotatingLaser(speedMultiplier);
                    break;
            }
        }

        private void ConfigureRotatingSpikes(float speedMultiplier)
        {
            PlayFrames("blades", 12f * speedMultiplier);
            _renderer.sprite = _renderer.sprite ?? ChallengeAssetCatalog.LoadHazard("blades/blades_00");
            transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            _rotationSpeed = 105f * speedMultiplier;
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.54f;
            _collider = collider;
            gameObject.tag = "Thorn";
        }

        private void ConfigurePopUpSpikes(float speedMultiplier)
        {
            _renderer.sprite = ChallengeAssetCatalog.LoadHazard("spike");
            transform.localScale = new Vector3(1.8f, 0.8f, 1f);
            transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.85f, 0.42f);
            _collider = collider;
            gameObject.tag = "Thorn";
            var moveDuration = 0.28f / Mathf.Max(0.75f, speedMultiplier);
            _motionTween = DOTween.Sequence()
                .AppendInterval(0.75f / speedMultiplier)
                .Append(transform.DOLocalMoveY(0.95f, moveDuration).SetEase(Ease.OutQuad))
                .AppendInterval(0.7f / speedMultiplier)
                .Append(transform.DOLocalMoveY(0.55f, moveDuration).SetEase(Ease.InQuad))
                .SetLoops(-1);
        }

        private void ConfigureStickySurface()
        {
            _renderer.sprite = ChallengeAssetCatalog.LoadHazard("sticky");
            _renderer.color = new Color(0.55f, 1f, 0.25f, 0.9f);
            transform.localScale = new Vector3(1.45f, 0.55f, 1f);
            transform.localPosition = new Vector3(0f, 0.88f, 0f);
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.2f, 0.36f);
            _collider = collider;
            gameObject.tag = "Untagged";
            _motionTween = transform.DOScaleY(0.63f, 0.55f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ConfigureRotatingLaser(float speedMultiplier)
        {
            PlayFrames("barrier", 11f * speedMultiplier);
            _renderer.sprite = _renderer.sprite ?? ChallengeAssetCatalog.LoadHazard("barrier/barrier_00");
            transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            _rotationSpeed = 72f * speedMultiplier;
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.75f, 0.18f);
            _collider = collider;
            gameObject.tag = "Thorn";
        }

        private void PlayFrames(string path, float framesPerSecond)
        {
            var frames = ChallengeAssetCatalog.LoadHazardFrames(path);
            if (frames.Length == 0)
                return;

            if (_frameAnimator == null)
                _frameAnimator = gameObject.AddComponent<FutureCityFrameAnimator>();
            _frameAnimator.Play(_renderer, frames, framesPerSecond);
        }

        private void Update()
        {
            if (_rotationSpeed != 0f)
                transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
        }

        public void Clear()
        {
            _motionTween?.Kill();
            _motionTween = null;
            _frameAnimator?.Stop();
            _rotationSpeed = 0f;
            _type = ThornType.None;
            if (_collider != null)
                Destroy(_collider);
            _collider = null;
            if (_renderer != null)
            {
                _renderer.sprite = null;
                _renderer.color = Color.white;
            }
            gameObject.SetActive(false);
        }
    }
}
