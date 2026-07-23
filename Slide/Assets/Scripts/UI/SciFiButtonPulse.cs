using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SciFiButtonPulse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Graphic _graphic;
        [SerializeField] private float _minAlpha = 0.78f;
        [SerializeField] private float _maxAlpha = 1f;
        [SerializeField] private float _pulseTime = 0.86f;
        [SerializeField] private float _hoverScale = 1.04f;

        private RectTransform _rectTransform;
        private Vector3 _baseScale;
        private Sequence _pulse;
        private bool _hovered;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _baseScale = _rectTransform.localScale;
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            StartPulse();
        }

        private void OnDisable()
        {
            KillTweens();
            if (_rectTransform != null)
                _rectTransform.localScale = _baseScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            ScaleTo(_hoverScale, 0.12f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            ScaleTo(1f, 0.12f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(0.96f, 0.08f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ScaleTo(_hovered ? _hoverScale : 1f, 0.1f);
        }

        private void StartPulse()
        {
            KillTweens();

            if (_graphic == null)
                return;

            var color = _graphic.color;
            color.a = _minAlpha;
            _graphic.color = color;

            _pulse = DOTween.Sequence().SetUpdate(true);
            _pulse.Append(_graphic.DOFade(_maxAlpha, _pulseTime).SetEase(Ease.InOutSine));
            _pulse.Append(_graphic.DOFade(_minAlpha, _pulseTime).SetEase(Ease.InOutSine));
            _pulse.SetLoops(-1);
        }

        private void ScaleTo(float scale, float duration)
        {
            if (_rectTransform == null)
                return;

            _rectTransform.DOKill();
            _rectTransform.DOScale(_baseScale * scale, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void KillTweens()
        {
            _pulse?.Kill();
            _pulse = null;

            if (_graphic != null)
                _graphic.DOKill();
            if (_rectTransform != null)
                _rectTransform.DOKill();
        }
    }
}
