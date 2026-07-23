using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ContinueOfferView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _timerFill;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _rewardedButton;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private RectTransform _skipTransform;

        private Coroutine _countdown;
        private Action _skipAction;
        private Action _rewardedAction;
        private Action _coinsAction;
        private Vector2 _skipPosition;
        private bool _isResolving;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_skipTransform != null)
                _skipPosition = _skipTransform.anchoredPosition;
        }

        public void Configure(Action skipAction, Action rewardedAction, Action coinsAction)
        {
            _skipAction = skipAction;
            _rewardedAction = rewardedAction;
            _coinsAction = coinsAction;

            Bind(_skipButton, Skip);
            Bind(_rewardedButton, SelectRewarded);
            Bind(_coinsButton, SelectCoins);
        }

        public void Show(
            float duration,
            float skipDelay,
            int score,
            int level,
            int balance,
            int price,
            bool rewardedAvailable)
        {
            gameObject.SetActive(true);
            _isResolving = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            _scoreText.text = $"СЧЕТ {score}";
            _levelText.text = $"УРОВЕНЬ {level}";
            _balanceText.text = balance.ToString();
            _priceText.text = price.ToString();
            _statusText.text = rewardedAvailable ? string.Empty : "РЕКЛАМА НЕДОСТУПНА";

            _rewardedButton.interactable = rewardedAvailable;
            _coinsButton.interactable = balance >= price;
            _skipButton.gameObject.SetActive(false);

            if (_skipTransform != null)
            {
                _skipTransform.DOKill();
                _skipTransform.anchoredPosition = _skipPosition;
            }

            if (_countdown != null)
                StopCoroutine(_countdown);
            _countdown = StartCoroutine(Countdown(duration, skipDelay));
        }

        public void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
        }

        public void Hide()
        {
            CancelCountdown();
            gameObject.SetActive(false);
        }

        private IEnumerator Countdown(float duration, float skipDelay)
        {
            var elapsed = 0f;
            UpdateTimer(duration, duration);

            while (elapsed < duration && !_isResolving)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateTimer(Mathf.Max(0f, duration - elapsed), duration);

                if (elapsed >= skipDelay && !_skipButton.gameObject.activeSelf)
                    RevealSkip();

                yield return null;
            }

            _countdown = null;
            if (!_isResolving)
                ResolveSkip(false);
        }

        private void UpdateTimer(float remaining, float duration)
        {
            _timerText.text = Mathf.CeilToInt(remaining).ToString();
            _timerFill.fillAmount = duration <= 0f ? 0f : remaining / duration;
        }

        private void RevealSkip()
        {
            _skipButton.gameObject.SetActive(true);
            if (_skipTransform == null)
                return;

            _skipTransform.anchoredPosition = _skipPosition - new Vector2(0f, 12f);
            _skipTransform.DOAnchorPos(_skipPosition, 0.24f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void Skip()
        {
            ResolveSkip(true);
        }

        private void ResolveSkip(bool animate)
        {
            if (_isResolving)
                return;

            _isResolving = true;
            CancelCountdown();
            SetButtonsInteractable(false);

            if (!animate || _skipTransform == null)
            {
                _skipAction?.Invoke();
                return;
            }

            _skipTransform.DOKill();
            _skipTransform.DOAnchorPosY(_skipPosition.y + 28f, 0.2f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() => _skipAction?.Invoke());
        }

        private void SelectRewarded()
        {
            if (!_rewardedButton.interactable || _isResolving)
                return;

            _rewardedAction?.Invoke();
        }

        private void SelectCoins()
        {
            if (!_coinsButton.interactable || _isResolving)
                return;

            _isResolving = true;
            CancelCountdown();
            SetButtonsInteractable(false);
            _coinsAction?.Invoke();
        }

        private void CancelCountdown()
        {
            if (_countdown == null)
                return;

            StopCoroutine(_countdown);
            _countdown = null;
        }

        private void SetButtonsInteractable(bool value)
        {
            _skipButton.interactable = value;
            _rewardedButton.interactable = value;
            _coinsButton.interactable = value;
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void OnDisable()
        {
            CancelCountdown();
            if (_skipTransform != null)
                _skipTransform.DOKill();
        }
    }
}
