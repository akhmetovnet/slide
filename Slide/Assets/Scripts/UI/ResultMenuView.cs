using System;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class ResultMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _recordText;
        [SerializeField] private TMP_Text _missionText;
        [SerializeField] private PixelNumberView _scoreView;
        [SerializeField] private Image _primaryImage;
        [SerializeField] private TMP_Text _primaryButtonText;
        [SerializeField] private Button _primaryButton;
        [SerializeField] private Button _charactersButton;
        [SerializeField] private Button _missionsButton;
        [SerializeField] private Sprite _restartNormal;
        [SerializeField] private Sprite _restartPressed;
        [SerializeField] private Sprite _continueNormal;
        [SerializeField] private Sprite _continuePressed;
        [SerializeField] private GameObject _storeOfferRoot;
        [SerializeField] private GameObject _noAdsRoot;
        [SerializeField] private Button _noAdsButton;

        private Action<bool> _primaryAction;
        private Action _charactersAction;
        private Action _missionsAction;
        private Action<string> _purchaseAction;
        private bool _isWin;
        private bool _primaryInvoked;

        public void Configure(
            Action<bool> primaryAction,
            Action charactersAction,
            Action missionsAction,
            Action<string> purchaseAction)
        {
            _primaryAction = primaryAction;
            _charactersAction = charactersAction;
            _missionsAction = missionsAction;
            _purchaseAction = purchaseAction;

            Bind(_primaryButton, SelectPrimary);
            Bind(_charactersButton, () => _charactersAction?.Invoke());
            Bind(_missionsButton, () => _missionsAction?.Invoke());
            Bind(_noAdsButton, () => _purchaseAction?.Invoke("no_ads"));
        }

        public void Show(
            bool isWin,
            GameMode mode,
            int balance,
            int score,
            int record,
            int missionNumber,
            RestartOfferDefinition offer,
            string offerPrice,
            bool offerAvailable,
            bool showNoAds,
            bool noAdsAvailable)
        {
            gameObject.SetActive(true);
            _isWin = isWin;
            _primaryInvoked = false;
            _primaryButton.interactable = true;

            _balanceText.text = $"БАЛАНС: {balance}";
            _scoreView.SetValue(score, true);
            _recordText.text = $"РЕКОРД: {record}М";

            var isChallenge = mode == GameMode.Challenge;
            _missionText.gameObject.SetActive(isChallenge);
            if (isChallenge)
                _missionText.text = $"МИССИЯ {missionNumber} — {(isWin ? "ПРОЙДЕНА" : "ПРОВАЛЕНА")}";

            ConfigurePrimaryButton(isWin);
            ConfigureOffer(offer, offerPrice, offerAvailable);

            _noAdsRoot.SetActive(showNoAds);
            if (showNoAds)
            {
                _noAdsButton.interactable = noAdsAvailable;
            }
        }

        private void ConfigurePrimaryButton(bool isWin)
        {
            var normal = isWin ? _continueNormal : _restartNormal;
            var pressed = isWin ? _continuePressed : _restartPressed;
            _primaryImage.sprite = normal;
            _primaryImage.preserveAspect = true;

            var state = _primaryButton.spriteState;
            state.pressedSprite = pressed;
            state.selectedSprite = pressed;
            _primaryButton.spriteState = state;
            _primaryButtonText.gameObject.SetActive(isWin);
            _primaryButtonText.text = isWin ? "СЛЕДУЮЩИЙ" : string.Empty;
        }

        private void ConfigureOffer(
            RestartOfferDefinition offer,
            string price,
            bool available)
        {
            // The future IAP slot intentionally stays neutral until product art and copy are approved.
            if (_storeOfferRoot != null)
                _storeOfferRoot.SetActive(true);
        }

        private void SelectPrimary()
        {
            if (_primaryInvoked)
                return;

            _primaryInvoked = true;
            _primaryButton.interactable = false;
            _primaryAction?.Invoke(_isWin);
        }

        public void SetNoAdsEntitled()
        {
            if (_noAdsRoot != null)
                _noAdsRoot.SetActive(false);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
