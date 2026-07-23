using UnityEngine;

namespace GameLogic
{
    public sealed class FutureCityFrameAnimator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _frames;
        private float _framesPerSecond;
        private float _time;
        private bool _isPlaying;

        public void Play(SpriteRenderer target, Sprite[] frames, float framesPerSecond, float phase = 0f)
        {
            _renderer = target;
            _frames = frames;
            _framesPerSecond = Mathf.Max(1f, framesPerSecond);
            _time = Mathf.Max(0f, phase);
            _isPlaying = _renderer != null && _frames != null && _frames.Length > 0;
            enabled = _isPlaying;

            if (_isPlaying)
                UpdateFrame();
        }

        public void Stop()
        {
            _isPlaying = false;
            enabled = false;
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _time += Time.deltaTime;
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            var index = Mathf.FloorToInt(_time * _framesPerSecond) % _frames.Length;
            _renderer.sprite = _frames[index];
        }
    }
}
