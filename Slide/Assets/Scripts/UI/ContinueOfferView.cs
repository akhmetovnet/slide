using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum ContinueOfferState
    {
        Hidden,
        Opening,
        Countdown,
        SkipAvailable,
        ContinueProcessing,
        Closing,
        Closed
    }

    public sealed class ContinueOfferView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _timerFill;
        [SerializeField] private TMP_Text _collectedText;
        [SerializeField] private TMP_Text _passedText;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _rewardedButton;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private GameObject _skipNormal;
        [SerializeField] private GameObject _skipPressed;
        [SerializeField] private RectTransform _skipNormalTransform;

        // Serialized by the previous RestartContinueV2 hierarchy. Keep these
        // references until all existing scenes have been migrated.
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private RectTransform _skipTransform;

        private Coroutine _countdown;
        private Action _skipAction;
        private Action _rewardedAction;
        private Func<bool> _coinsAction;
        private Vector2 _skipPosition;
        private float _skipCloseDuration;

        public ContinueOfferState State { get; private set; } = ContinueOfferState.Hidden;

        private void Awake()
        {
            NormalizeNestedReferenceCanvas();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            ConfigureLegacyHierarchy();

            if (_skipNormalTransform != null)
                _skipPosition = _skipNormalTransform.anchoredPosition;
        }

        public bool CanShow =>
            _timerFill != null &&
            _balanceText != null &&
            _priceText != null &&
            _rewardedButton != null &&
            _coinsButton != null &&
            _skipButton != null;

        public void Configure(Action skipAction, Action rewardedAction, Func<bool> coinsAction)
        {
            _skipAction = skipAction;
            _rewardedAction = rewardedAction;
            _coinsAction = coinsAction;
            Bind(_skipButton, SelectSkip);
            Bind(_rewardedButton, SelectRewarded);
            Bind(_coinsButton, SelectCoins);
        }

        public void Show(
            float duration,
            float skipDelay,
            float skipCloseDuration,
            int collected,
            int passed,
            int balance,
            int price,
            bool coinsAvailable)
        {
            ConfigureLegacyHierarchy();
            if (!CanShow)
            {
                Debug.LogError("ContinueOfferView is missing required UI references.");
                return;
            }

            CancelCountdown();
            KillSkipTween();
            gameObject.SetActive(true);
            State = ContinueOfferState.Opening;
            _skipCloseDuration = Mathf.Clamp(skipCloseDuration, 0.12f, 0.2f);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_collectedText != null)
                _collectedText.text = collected.ToString();
            if (_passedText != null)
                _passedText.text = $"{passed} M";
            _balanceText.text = balance.ToString();
            _priceText.text = price.ToString();
            if (_statusText != null)
                _statusText.text = string.Empty;
            _rewardedButton.interactable = true;
            _coinsButton.interactable = coinsAvailable;
            _skipButton.interactable = false;
            if (_skipNormal != null)
                _skipNormal.SetActive(false);
            else
                _skipButton.gameObject.SetActive(false);
            if (_skipPressed != null)
                _skipPressed.SetActive(false);

            if (_skipNormalTransform != null)
                _skipNormalTransform.anchoredPosition = _skipPosition;

            _countdown = StartCoroutine(Countdown(Mathf.Max(0f, duration), Mathf.Max(0f, skipDelay)));
        }

        public void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
        }

        public void SetBalance(int balance)
        {
            _balanceText.text = balance.ToString();
        }

        public void Close()
        {
            CancelCountdown();
            KillSkipTween();
            State = ContinueOfferState.Closed;
            gameObject.SetActive(false);
        }

        private IEnumerator Countdown(float duration, float skipDelay)
        {
            yield return null;
            if (State != ContinueOfferState.Opening)
                yield break;

            State = ContinueOfferState.Countdown;
            var elapsed = 0f;
            UpdateTimer(duration, duration);

            while (elapsed < duration && IsCountingDown)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateTimer(Mathf.Max(0f, duration - elapsed), duration);

                if (elapsed >= skipDelay && State == ContinueOfferState.Countdown)
                    RevealSkip();

                yield return null;
            }

            _countdown = null;
            if (IsCountingDown)
                BeginClosing(false);
        }

        private bool IsCountingDown => State == ContinueOfferState.Countdown || State == ContinueOfferState.SkipAvailable;

        private void UpdateTimer(float remaining, float duration)
        {
            _timerFill.fillAmount = duration <= 0f ? 0f : Mathf.Clamp01(remaining / duration);
            if (_timerText != null)
                _timerText.text = Mathf.CeilToInt(remaining).ToString();
        }

        private void RevealSkip()
        {
            State = ContinueOfferState.SkipAvailable;
            if (_skipNormal != null)
                _skipNormal.SetActive(true);
            else
                _skipButton.gameObject.SetActive(true);
            _skipButton.interactable = true;
        }

        private void SelectRewarded()
        {
            if (!IsCountingDown || !_rewardedButton.interactable)
                return;

            BeginContinueProcessing();
            _rewardedAction?.Invoke();
        }

        private void SelectCoins()
        {
            if (!IsCountingDown || !_coinsButton.interactable)
                return;

            if (_coinsAction == null || !_coinsAction.Invoke())
                return;

            BeginContinueProcessing();
        }

        private void SelectSkip()
        {
            if (!IsCountingDown || !_skipButton.interactable)
                return;

            BeginClosing(true);
        }

        private void BeginContinueProcessing()
        {
            State = ContinueOfferState.ContinueProcessing;
            CancelCountdown();
            SetButtonsInteractable(false);
        }

        private void BeginClosing(bool animateSkip)
        {
            if (State == ContinueOfferState.Closing || State == ContinueOfferState.Closed)
                return;

            State = ContinueOfferState.Closing;
            CancelCountdown();
            SetButtonsInteractable(false);

            if (!animateSkip || _skipNormalTransform == null)
            {
                _skipAction?.Invoke();
                return;
            }

            if (_skipPressed != null)
                _skipPressed.SetActive(true);
            _skipNormalTransform.DOAnchorPosY(_skipPosition.y + 20f, _skipCloseDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() => _skipAction?.Invoke());
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
            _rewardedButton.interactable = value;
            _coinsButton.interactable = value;
            _skipButton.interactable = value;
        }

        private void KillSkipTween()
        {
            if (_skipNormalTransform != null)
                _skipNormalTransform.DOKill();
        }

        private void ConfigureLegacyHierarchy()
        {
            if (_scoreText != null)
                _scoreText.gameObject.SetActive(false);
            if (_levelText != null)
                _levelText.gameObject.SetActive(false);

            if (_skipNormalTransform == null)
                _skipNormalTransform = _skipTransform;

            if (_priceText == null || _coinsButton == null)
                return;

            var legacyPriceMask = _priceText.transform.parent as RectTransform;
            if (legacyPriceMask == null || legacyPriceMask.name != "PriceMask")
                return;

            // The old mask was positioned below the visible button. Move the
            // same dynamic price area into the button instead of adding a
            // second price label or another panel.
            legacyPriceMask.anchorMin = legacyPriceMask.anchorMax = legacyPriceMask.pivot =
                new Vector2(0.5f, 0.5f);
            legacyPriceMask.anchoredPosition = new Vector2(8f, -4f);
            var maskImage = legacyPriceMask.GetComponent<Image>();
            if (maskImage != null)
                maskImage.color = new Color32(43, 225, 228, 255);
        }

        private void NormalizeNestedReferenceCanvas()
        {
            var nestedCanvas = GetComponent<Canvas>();
            if (nestedCanvas == null)
                return;

            nestedCanvas.enabled = false;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
                scaler.enabled = false;
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                raycaster.enabled = false;

            var safeRoot = transform.Find("SafeArea") as RectTransform;
            if (safeRoot == null || safeRoot.Find("Artboard") != null)
                return;

            var artboardObject = new GameObject("Artboard", typeof(RectTransform), typeof(AspectRatioFitter));
            var artboard = artboardObject.transform as RectTransform;
            artboard.SetParent(safeRoot, false);
            artboard.anchorMin = Vector2.zero;
            artboard.anchorMax = Vector2.one;
            artboard.offsetMin = Vector2.zero;
            artboard.offsetMax = Vector2.zero;

            var fitter = artboardObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1280f / 2304f;

            var children = new List<RectTransform>();
            foreach (Transform child in safeRoot)
            {
                if (child == artboard || child.name == "Backdrop")
                    continue;
                if (child is RectTransform rect)
                    children.Add(rect);
            }

            foreach (var child in children)
            {
                child.SetParent(artboard, false);
                if (IsFullReferenceLayer(child.name))
                {
                    child.anchorMin = Vector2.zero;
                    child.anchorMax = Vector2.one;
                    child.offsetMin = Vector2.zero;
                    child.offsetMax = Vector2.zero;
                    continue;
                }

                NormalizeReferenceRect(child);
                var text = child.GetComponent<TMP_Text>();
                if (text != null)
                    text.fontSize *= 0.25f;
            }
        }

        private static bool IsFullReferenceLayer(string objectName)
        {
            return objectName == "Plate" ||
                   objectName == "Balance" ||
                   objectName == "Collected" ||
                   objectName == "Passed" ||
                   objectName == "RewardedArt" ||
                   objectName == "CoinsArt" ||
                   objectName == "Timer" ||
                   objectName == "SkipNormal" ||
                   objectName == "SkipPressed";
        }

        private static void NormalizeReferenceRect(RectTransform rect)
        {
            var position = rect.anchoredPosition;
            var size = rect.sizeDelta;
            var x = position.x + 640f;
            var y = 1152f - position.y;
            if (rect.name.EndsWith("ValueMask", StringComparison.Ordinal))
                x += size.x * 0.5f;
            if (rect.name == "RewardedContinue" ||
                rect.name == "CoinsContinue" ||
                rect.name == "Skip")
            {
                x += size.x * 0.5f;
                y += size.y * 0.5f;
            }

            rect.anchorMin = new Vector2(
                (x - size.x * 0.5f) / 1280f,
                1f - (y + size.y * 0.5f) / 2304f);
            rect.anchorMax = new Vector2(
                (x + size.x * 0.5f) / 1280f,
                1f - (y - size.y * 0.5f) / 2304f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            KillSkipTween();
            State = ContinueOfferState.Closed;
        }
    }
}
