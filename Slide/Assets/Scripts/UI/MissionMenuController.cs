using DG.Tweening;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public sealed class MissionMenuController : MonoBehaviour
    {
        private const string GameSceneName = "Game";

        [Header("Map")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _glass;
        [SerializeField] private CanvasGroup _futureCityBackgroundGroup;
        [SerializeField] private CanvasGroup _mapGroup;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _mapRoot;
        [SerializeField] private RectTransform _selectionFrame;
        [SerializeField] private Sprite[] _backgroundSprites;
        [SerializeField] private Image[] _connectors;
        [SerializeField] private Button[] _missionButtons;
        [SerializeField] private Image[] _missionImages;
        [SerializeField] private TMP_Text[] _missionLabels;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _activePressedSprite;
        [SerializeField] private Sprite _completedSprite;
        [SerializeField] private Sprite _completedPressedSprite;
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private RectTransform _activeArrow;
        [SerializeField] private Image _activeArrowImage;
        [SerializeField] private Sprite[] _activeArrowFrames;

        [Header("Top controls")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _storeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _noAdsButton;

        [Header("Prepare window")]
        [SerializeField] private GameObject _preparePanel;
        [SerializeField] private CanvasGroup _prepareGroup;
        [SerializeField] private TMP_Text _prepareMission;
        [SerializeField] private TMP_Text _prepareObjective;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _cancelPrepareButton;

        [Header("Settings")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private CanvasGroup _settingsGroup;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _vibrationToggle;
        [SerializeField] private Button _closeSettingsButton;

        [Header("Transition")]
        [SerializeField] private Image _transitionFlash;
        [SerializeField] private Image _lightningLine;

        private int _selectedLevel;
        private int _displayedLocation = -1;
        private bool _isTransitioning;
        private Vector2 _arrowBasePosition;
        private float _nextLocationCheckTime;

        private void Awake()
        {
            BindButtons();
        }

        private void Start()
        {
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            ChallengeProgress.Initialize();
            _selectedLevel = Mathf.Clamp(
                ChallengeProgress.SelectedLevel,
                1,
                ChallengeLevelCatalog.LevelCount);

            if (_preparePanel != null)
                _preparePanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_storeButton != null)
                _storeButton.gameObject.SetActive(false);
            SetAlpha(_transitionFlash, 0f);
            SetAlpha(_lightningLine, 0f);

            if (_soundToggle != null)
                _soundToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Sounds", 1) == 1);
            if (_vibrationToggle != null)
                _vibrationToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Vibration", 1) == 1);

            RefreshMap();
            PlayEntryAnimation();
            AnimateNewlyCompletedMission();
        }

        private void Update()
        {
            AnimateArrow();

            if (_glass != null)
            {
                var glow = Mathf.Sin(Time.unscaledTime * 0.65f) * 0.5f + 0.5f;
                SetAlpha(_glass, Mathf.Lerp(0.2f, 0.34f, glow));
            }

            if (Time.unscaledTime >= _nextLocationCheckTime)
            {
                _nextLocationCheckTime = Time.unscaledTime + 0.12f;
                UpdateVisibleLocation();
            }

            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (_preparePanel != null && _preparePanel.activeSelf)
                HidePreparePanel();
            else if (_settingsPanel != null && _settingsPanel.activeSelf)
                HideSettings();
            else
                BackToGame();
        }

        private void OnDisable()
        {
            transform.DOKill();
            _background?.DOKill();
            _futureCityBackgroundGroup?.DOKill();
            _mapGroup?.DOKill();
            _mapRoot?.DOKill();
            _activeArrow?.DOKill();
            _prepareGroup?.DOKill();
            _settingsGroup?.DOKill();
            _transitionFlash?.DOKill();
            _lightningLine?.DOKill();
        }

        public void ShowPreparePanel()
        {
            if (_isTransitioning || !ChallengeProgress.IsUnlocked(_selectedLevel))
                return;

            var definition = ChallengeLevelCatalog.Get(_selectedLevel);
            if (_prepareMission != null)
                _prepareMission.text = $"МИССИЯ {_selectedLevel}";
            if (_prepareObjective != null)
            {
                _prepareObjective.text =
                    $"{definition.GetTitle()}\n\nКАСАЙТЕСЬ ЭКРАНА, ЧТОБЫ СПУСКАТЬСЯ НИЖЕ.\nИЗБЕГАЙТЕ ПРЕПЯТСТВИЙ.";
            }

            if (_preparePanel == null || _prepareGroup == null)
                return;

            _preparePanel.SetActive(true);
            _prepareGroup.alpha = 0f;
            _prepareGroup.interactable = true;
            _prepareGroup.blocksRaycasts = true;
            _prepareGroup.DOFade(1f, 0.22f).SetUpdate(true);
            _prepareGroup.transform.localScale = Vector3.one * 0.9f;
            _prepareGroup.transform.DOScale(Vector3.one, 0.28f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void HidePreparePanel()
        {
            if (_preparePanel == null || !_preparePanel.activeSelf || _prepareGroup == null)
                return;

            _prepareGroup.interactable = false;
            _prepareGroup.blocksRaycasts = false;
            _prepareGroup.DOFade(0f, 0.16f)
                .SetUpdate(true)
                .OnComplete(() => _preparePanel.SetActive(false));
        }

        public void StartSelectedMission()
        {
            if (_isTransitioning || !ChallengeProgress.SelectLevel(_selectedLevel))
                return;

            _isTransitioning = true;
            PlayerPrefs.SetInt(ChallengeProgress.AutoStartKey, 1);
            PlayerPrefs.Save();
            PlayLightningTransition(() => SceneManager.LoadScene(GameSceneName));
        }

        public void BackToGame()
        {
            if (!_isTransitioning)
                SceneManager.LoadScene(GameSceneName);
        }

        public void ShowSettings()
        {
            if (_settingsPanel == null || _settingsGroup == null || _isTransitioning)
                return;

            _settingsPanel.SetActive(true);
            _settingsGroup.alpha = 0f;
            _settingsGroup.interactable = true;
            _settingsGroup.blocksRaycasts = true;
            _settingsGroup.DOFade(1f, 0.2f).SetUpdate(true);
            _settingsGroup.transform.localScale = Vector3.one * 0.9f;
            _settingsGroup.transform.DOScale(Vector3.one, 0.26f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void HideSettings()
        {
            if (_settingsPanel == null || !_settingsPanel.activeSelf || _settingsGroup == null)
                return;

            _settingsGroup.interactable = false;
            _settingsGroup.blocksRaycasts = false;
            _settingsGroup.DOFade(0f, 0.16f)
                .SetUpdate(true)
                .OnComplete(() => _settingsPanel.SetActive(false));
        }

        public void PulseNoAdsButton()
        {
            if (_noAdsButton == null)
                return;

            _noAdsButton.transform.DOKill();
            _noAdsButton.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 8, 0.5f)
                .SetUpdate(true);
        }

        private void BindButtons()
        {
            if (_missionButtons != null)
            {
                for (var i = 0; i < _missionButtons.Length; i++)
                {
                    var slot = i;
                    _missionButtons[slot]?.onClick.AddListener(() => SelectMissionSlot(slot));
                }
            }

            _backButton?.onClick.AddListener(BackToGame);
            _settingsButton?.onClick.AddListener(ShowSettings);
            _noAdsButton?.onClick.AddListener(PulseNoAdsButton);
            _startButton?.onClick.AddListener(StartSelectedMission);
            _cancelPrepareButton?.onClick.AddListener(HidePreparePanel);
            _closeSettingsButton?.onClick.AddListener(HideSettings);

            if (_soundToggle != null)
            {
                _soundToggle.onValueChanged.AddListener(value =>
                {
                    PlayerPrefs.SetInt("Sounds", value ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            if (_vibrationToggle != null)
            {
                _vibrationToggle.onValueChanged.AddListener(value =>
                {
                    PlayerPrefs.SetInt("Vibration", value ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }
        }

        private void SelectMissionSlot(int slot)
        {
            if (_isTransitioning || slot < 0 || slot >= ChallengeLevelCatalog.LevelCount)
                return;

            var level = slot + 1;
            if (!ChallengeProgress.IsUnlocked(level))
            {
                var buttonTransform = _missionButtons[slot].transform;
                buttonTransform.DOKill();
                buttonTransform.DOPunchPosition(new Vector3(22f, 0f, 0f), 0.32f, 12, 0.6f)
                    .SetUpdate(true);
                return;
            }

            _selectedLevel = level;
            ChallengeProgress.SelectLevel(level);
            PositionSelection(slot);
            SetDisplayedLocation(ChallengeLevelCatalog.Get(level).Location, true);
            ShowPreparePanel();
        }

        private void RefreshMap()
        {
            var unlockedLevel = ChallengeProgress.HighestUnlockedLevel;
            var nodeCount = Mathf.Min(
                ChallengeLevelCatalog.LevelCount,
                Mathf.Min(_missionButtons?.Length ?? 0, _missionImages?.Length ?? 0));

            for (var i = 0; i < nodeCount; i++)
            {
                var level = i + 1;
                var state = level < unlockedLevel
                    ? NodeState.Completed
                    : level == unlockedLevel
                        ? NodeState.Active
                        : NodeState.Locked;
                ApplyNodeState(i, state);
            }

            if (_connectors != null)
            {
                for (var i = 0; i < _connectors.Length; i++)
                {
                    if (_connectors[i] != null)
                        _connectors[i].gameObject.SetActive(i < unlockedLevel - 1);
                }
            }

            _selectedLevel = Mathf.Clamp(_selectedLevel, 1, Mathf.Max(1, nodeCount));
            PositionSelection(_selectedLevel - 1);
            SetDisplayedLocation(ChallengeLevelCatalog.Get(_selectedLevel).Location, false);
            ScrollToMission(_selectedLevel, false);
        }

        private void ApplyNodeState(int slot, NodeState state)
        {
            if (slot < 0 ||
                _missionImages == null ||
                _missionButtons == null ||
                slot >= _missionImages.Length ||
                slot >= _missionButtons.Length)
            {
                return;
            }

            var image = _missionImages[slot];
            var button = _missionButtons[slot];
            if (image == null || button == null)
                return;

            var spriteState = button.spriteState;
            switch (state)
            {
                case NodeState.Completed:
                    image.sprite = _completedSprite;
                    spriteState.pressedSprite = _completedPressedSprite;
                    spriteState.selectedSprite = _completedPressedSprite;
                    break;
                case NodeState.Active:
                    image.sprite = _activeSprite;
                    spriteState.pressedSprite = _activePressedSprite;
                    spriteState.selectedSprite = _activePressedSprite;
                    break;
                default:
                    image.sprite = _lockedSprite;
                    spriteState.pressedSprite = _lockedSprite;
                    spriteState.selectedSprite = _lockedSprite;
                    break;
            }

            button.interactable = true;
            button.spriteState = spriteState;
        }

        private void PositionSelection(int slot)
        {
            if (_missionButtons == null || slot < 0 || slot >= _missionButtons.Length)
                return;

            var nodeRect = _missionButtons[slot].transform as RectTransform;
            if (nodeRect == null)
                return;

            if (_missionLabels != null)
            {
                for (var i = 0; i < _missionLabels.Length; i++)
                {
                    if (_missionLabels[i] != null)
                        _missionLabels[i].text = i == slot ? $"#{i + 1}" : string.Empty;
                }
            }

            if (_selectionFrame != null)
            {
                _selectionFrame.gameObject.SetActive(true);
                _selectionFrame.anchoredPosition = nodeRect.anchoredPosition;
                _selectionFrame.SetSiblingIndex(Mathf.Max(0, nodeRect.GetSiblingIndex() - 1));
            }

            if (_activeArrow == null)
                return;

            _activeArrow.gameObject.SetActive(true);
            _arrowBasePosition = nodeRect.anchoredPosition + new Vector2(0f, 128f);
            _activeArrow.anchoredPosition = _arrowBasePosition;
            _activeArrow.SetAsLastSibling();
        }

        private void ScrollToMission(int level, bool animated)
        {
            if (_mapRoot == null || _viewport == null || _missionButtons == null)
                return;

            var slot = Mathf.Clamp(level - 1, 0, _missionButtons.Length - 1);
            var node = _missionButtons[slot].transform as RectTransform;
            if (node == null)
                return;

            Canvas.ForceUpdateCanvases();
            var nodeDistanceFromTop = -node.anchoredPosition.y;
            var targetScreenY = Mathf.Min(690f, _viewport.rect.height * 0.32f);
            var maxScroll = Mathf.Max(0f, _mapRoot.rect.height - _viewport.rect.height);
            var targetY = Mathf.Clamp(nodeDistanceFromTop - targetScreenY, 0f, maxScroll);

            _mapRoot.DOKill();
            if (animated)
            {
                _mapRoot.DOAnchorPosY(targetY, 0.35f)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);
            }
            else
            {
                var position = _mapRoot.anchoredPosition;
                position.y = targetY;
                _mapRoot.anchoredPosition = position;
            }
        }

        private void UpdateVisibleLocation()
        {
            if (_mapRoot == null || _viewport == null || _missionButtons == null || _missionButtons.Length == 0)
                return;

            var probeDistance = _mapRoot.anchoredPosition.y + Mathf.Min(690f, _viewport.rect.height * 0.32f);
            var nearestSlot = 0;
            var nearestDistance = float.MaxValue;
            for (var i = 0; i < _missionButtons.Length; i++)
            {
                var node = _missionButtons[i].transform as RectTransform;
                if (node == null)
                    continue;

                var distance = Mathf.Abs(-node.anchoredPosition.y - probeDistance);
                if (distance >= nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestSlot = i;
            }

            SetDisplayedLocation(ChallengeLevelCatalog.Get(nearestSlot + 1).Location, true);
        }

        private void SetDisplayedLocation(ChallengeLocation location, bool animateDivider)
        {
            var locationId = (int)location;
            if (_displayedLocation == locationId)
                return;

            _displayedLocation = locationId;
            if (_background != null)
            {
                _background.sprite = GetBackground(location);
                SetAlpha(_background, 1f);
            }

            if (_futureCityBackgroundGroup != null)
            {
                var showFutureCityLayers = location == ChallengeLocation.FutureCity;
                _futureCityBackgroundGroup.gameObject.SetActive(showFutureCityLayers);
                _futureCityBackgroundGroup.alpha = showFutureCityLayers ? 1f : 0f;
            }

            if (animateDivider)
                FlashDivider();
        }

        private void AnimateArrow()
        {
            if (_activeArrow == null || !_activeArrow.gameObject.activeSelf)
                return;

            var offset = Mathf.Sin(Time.unscaledTime * 4.2f) * 10f;
            _activeArrow.anchoredPosition = _arrowBasePosition + new Vector2(0f, offset);
            if (_activeArrowImage == null || _activeArrowFrames == null || _activeArrowFrames.Length == 0)
                return;

            var frame = Mathf.FloorToInt(Time.unscaledTime * 8f) % _activeArrowFrames.Length;
            _activeArrowImage.sprite = _activeArrowFrames[frame];
        }

        private void PlayEntryAnimation()
        {
            if (_mapGroup == null)
                return;

            _mapGroup.alpha = 0f;
            _mapGroup.DOFade(1f, 0.38f).SetUpdate(true);
        }

        private void AnimateNewlyCompletedMission()
        {
            var unlockedLevel = ChallengeProgress.HighestUnlockedLevel;
            var seenUnlockedLevel = PlayerPrefs.GetInt(
                ChallengeProgress.SeenUnlockedLevelKey,
                unlockedLevel);
            PlayerPrefs.SetInt(ChallengeProgress.SeenUnlockedLevelKey, unlockedLevel);
            PlayerPrefs.Save();

            if (unlockedLevel <= seenUnlockedLevel || unlockedLevel <= 1)
                return;

            var slot = unlockedLevel - 2;
            if (_missionButtons == null || slot < 0 || slot >= _missionButtons.Length)
                return;

            _missionImages[slot].sprite = _activeSprite;
            var node = _missionButtons[slot].transform;
            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.AppendInterval(0.32f);
            sequence.Append(node.DOPunchScale(Vector3.one * 0.16f, 0.38f, 10, 0.65f));
            sequence.InsertCallback(0.44f, () => ApplyNodeState(slot, NodeState.Completed));
            sequence.InsertCallback(0.44f, FlashDivider);
        }

        private void PlayLightningTransition(TweenCallback onComplete)
        {
            if (_transitionFlash == null || _lightningLine == null)
            {
                onComplete?.Invoke();
                return;
            }

            _transitionFlash.gameObject.SetActive(true);
            _lightningLine.gameObject.SetActive(true);
            SetAlpha(_transitionFlash, 0f);
            SetAlpha(_lightningLine, 0f);
            _lightningLine.rectTransform.localScale = new Vector3(0.05f, 1f, 1f);

            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(_lightningLine.DOFade(1f, 0.05f));
            sequence.Join(_lightningLine.rectTransform.DOScaleX(1f, 0.14f).SetEase(Ease.OutExpo));
            sequence.Append(_transitionFlash.DOFade(0.92f, 0.08f));
            sequence.Append(_transitionFlash.DOFade(0.18f, 0.1f));
            sequence.Append(_transitionFlash.DOFade(1f, 0.16f));
            sequence.AppendCallback(onComplete);
        }

        private void FlashDivider()
        {
            if (_lightningLine == null)
                return;

            _lightningLine.gameObject.SetActive(true);
            _lightningLine.DOKill();
            SetAlpha(_lightningLine, 0f);
            _lightningLine.rectTransform.localScale = new Vector3(0.1f, 1f, 1f);
            _lightningLine.DOFade(0.85f, 0.05f).SetUpdate(true);
            _lightningLine.rectTransform.DOScaleX(1f, 0.16f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
                .OnComplete(() => _lightningLine.DOFade(0f, 0.18f).SetUpdate(true));
        }

        private Sprite GetBackground(ChallengeLocation location)
        {
            var config = LocationCatalog.Get(location);
            if (config != null && config.MissionMenuBackground != null)
                return config.MissionMenuBackground;

            if (_backgroundSprites == null || _backgroundSprites.Length == 0)
                return null;

            var legacyIndex = location == ChallengeLocation.FutureCity ? 0 :
                location == ChallengeLocation.Jungle ? 1 :
                location == ChallengeLocation.SpaceStation ? 2 : 3;
            return _backgroundSprites[Mathf.Clamp(legacyIndex, 0, _backgroundSprites.Length - 1)];
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private enum NodeState
        {
            Locked,
            Active,
            Completed
        }
    }
}
