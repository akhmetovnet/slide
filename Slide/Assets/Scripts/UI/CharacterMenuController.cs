using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class CharacterMenuController : MonoBehaviour
    {
        private const string CurrentSkinKey = "CurrentSkin";
        private const string CoinsKey = "Coins";
        private const string SkinKeyPrefix = "Skin";
        private const float TransitionDistance = 980f;
        private const float TransitionTime = 0.34f;

        [Header("Animated scene")]
        [SerializeField] private RectTransform _selectionRoot;
        [SerializeField] private CanvasGroup _selectionGroup;
        [SerializeField] private RectTransform _robotRoot;
        [SerializeField] private RectTransform _headTransform;
        [SerializeField] private RectTransform _armsTransform;
        [SerializeField] private RectTransform _legsTransform;
        [SerializeField] private RectTransform _leftFireTransform;
        [SerializeField] private RectTransform _rightFireTransform;
        [SerializeField] private Image _fan;
        [SerializeField] private Image _ringLight;
        [SerializeField] private Image _lockedCharacter;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private Image _headImage;
        [SerializeField] private Image _bodyImage;
        [SerializeField] private Image _armsImage;
        [SerializeField] private Image _legsImage;
        [SerializeField] private Image _leftFire;
        [SerializeField] private Image _rightFire;
        [SerializeField] private Image _podiumGlow1;
        [SerializeField] private Image _podiumGlow2;
        [SerializeField] private Image _podiumGlow3;
        [SerializeField] private Image[] _tintedRobotImages;

        [Header("UI")]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _stateText;
        [SerializeField] private TMP_Text _actionText;

        [Header("Data")]
        [SerializeField] private Sprite[] _skinSprites;
        [SerializeField] private int[] _prices;
        [SerializeField] private string[] _names;
        [SerializeField] private string[] _descriptions;
        [SerializeField] private Color[] _skinTints;
        [SerializeField] private string _gameSceneName = "Game";

        private readonly Color32[] _fallbackTints =
        {
            new Color32(70, 242, 255, 255),
            new Color32(255, 103, 54, 255),
            new Color32(75, 255, 118, 255),
            new Color32(114, 172, 255, 255),
            new Color32(255, 194, 61, 255),
            new Color32(111, 255, 219, 255),
            new Color32(78, 188, 255, 255),
            new Color32(255, 92, 92, 255),
            new Color32(129, 255, 84, 255),
            new Color32(255, 128, 214, 255),
            new Color32(166, 140, 255, 255),
            new Color32(242, 255, 255, 255)
        };

        private Vector2 _selectionBasePosition;
        private Vector2 _robotBasePosition;
        private Vector2 _headBasePosition;
        private Vector2 _armsBasePosition;
        private Vector2 _legsBasePosition;
        private Vector2 _leftFireBasePosition;
        private Vector2 _rightFireBasePosition;
        private int _currentIndex;
        private bool _isTransitioning;

        private int CharacterCount
        {
            get
            {
                var count = Mathf.Max(_skinSprites?.Length ?? 0, _prices?.Length ?? 0);
                count = Mathf.Max(count, _names?.Length ?? 0);
                count = Mathf.Max(count, _descriptions?.Length ?? 0);
                return Mathf.Max(count, 1);
            }
        }

        private void Awake()
        {
            CacheBasePositions();
            BindButtons();
        }

        private void Start()
        {
            PlayerPrefs.SetInt($"{SkinKeyPrefix}0", 1);
            _currentIndex = Mathf.Clamp(PlayerPrefs.GetInt(CurrentSkinKey, 0), 0, CharacterCount - 1);
            RefreshView(false, string.Empty);
        }

        private void Update()
        {
            var time = Time.unscaledTime;

            if (_fan != null)
                _fan.rectTransform.Rotate(0f, 0f, -58f * Time.unscaledDeltaTime);

            if (_ringLight != null)
                SetAlpha(_ringLight, Mathf.Lerp(0.42f, 0.95f, Pulse(time, 0.72f)));

            var strongHighlight = PlayerPrefs.GetInt(CurrentSkinKey, 0) == _currentIndex && IsUnlocked(_currentIndex);
            SetAlpha(_podiumGlow1, Mathf.Lerp(0.36f, strongHighlight ? 0.94f : 0.64f, Pulse(time, 1.55f)));
            SetAlpha(_podiumGlow2, Mathf.Lerp(0.22f, strongHighlight ? 0.82f : 0.48f, Pulse(time + 0.3f, 1.1f)));
            SetAlpha(_podiumGlow3, Mathf.Lerp(0.16f, strongHighlight ? 0.52f : 0.32f, Pulse(time + 0.6f, 0.9f)));

            if (_isTransitioning)
                return;

            var idle = Mathf.Sin(time * 2.35f);
            if (_robotRoot != null)
                _robotRoot.anchoredPosition = _robotBasePosition + new Vector2(0f, idle * 12f);
            if (_headTransform != null)
                _headTransform.anchoredPosition = _headBasePosition + new Vector2(0f, idle * 5f);
            if (_armsTransform != null)
                _armsTransform.localEulerAngles = new Vector3(0f, 0f, idle * 1.9f);
            if (_legsTransform != null)
                _legsTransform.anchoredPosition = _legsBasePosition + new Vector2(0f, -idle * 4f);

            var firePulse = Pulse(time, 2.8f);
            if (_leftFireTransform != null)
                _leftFireTransform.anchoredPosition = _leftFireBasePosition + new Vector2(0f, -firePulse * 8f);
            if (_rightFireTransform != null)
                _rightFireTransform.anchoredPosition = _rightFireBasePosition + new Vector2(0f, -firePulse * 8f);
            SetAlpha(_leftFire, Mathf.Lerp(0.38f, 0.86f, firePulse));
            SetAlpha(_rightFire, Mathf.Lerp(0.38f, 0.86f, 1f - firePulse));
        }

        private void OnDisable()
        {
            if (_selectionRoot != null)
                _selectionRoot.DOKill();
            if (_selectionGroup != null)
                _selectionGroup.DOKill();
            if (_actionButton != null)
                _actionButton.transform.DOKill();
        }

        public void NextCharacter()
        {
            SwitchCharacter(1);
        }

        public void PreviousCharacter()
        {
            SwitchCharacter(-1);
        }

        public void UpgradeSelectedCharacter()
        {
            if (_isTransitioning)
                return;

            if (IsUnlocked(_currentIndex))
            {
                PlayerPrefs.SetInt(CurrentSkinKey, _currentIndex);
                PlayerPrefs.Save();
                RefreshView(true, "МОДУЛЬ НАЗНАЧЕН");
                return;
            }

            var price = GetPrice(_currentIndex);
            if (price <= 0)
            {
                RefreshView(false, "МОДУЛЬ ЗАКРЫТ ДО СЛЕДУЮЩЕГО ОБНОВЛЕНИЯ");
                PulseDenied();
                return;
            }

            var coins = PlayerPrefs.GetInt(CoinsKey, 0);
            if (coins < price)
            {
                RefreshView(false, $"НЕ ХВАТАЕТ {price - coins} КАТУШЕК");
                PulseDenied();
                return;
            }

            PlayerPrefs.SetInt(CoinsKey, coins - price);
            PlayerPrefs.SetInt($"{SkinKeyPrefix}{_currentIndex}", 1);
            PlayerPrefs.SetInt(CurrentSkinKey, _currentIndex);
            PlayerPrefs.Save();
            RefreshView(true, "МОДУЛЬ АКТИВИРОВАН");
        }

        public void BackToGame()
        {
            if (!string.IsNullOrEmpty(_gameSceneName) && Application.CanStreamedLevelBeLoaded(_gameSceneName))
            {
                SceneManager.LoadScene(_gameSceneName);
                return;
            }

            SceneManager.LoadScene(0);
        }

        private void CacheBasePositions()
        {
            if (_selectionRoot != null)
                _selectionBasePosition = _selectionRoot.anchoredPosition;
            if (_robotRoot != null)
                _robotBasePosition = _robotRoot.anchoredPosition;
            if (_headTransform != null)
                _headBasePosition = _headTransform.anchoredPosition;
            if (_armsTransform != null)
                _armsBasePosition = _armsTransform.anchoredPosition;
            if (_legsTransform != null)
                _legsBasePosition = _legsTransform.anchoredPosition;
            if (_leftFireTransform != null)
                _leftFireBasePosition = _leftFireTransform.anchoredPosition;
            if (_rightFireTransform != null)
                _rightFireBasePosition = _rightFireTransform.anchoredPosition;
        }

        private void BindButtons()
        {
            if (_previousButton != null)
                _previousButton.onClick.AddListener(PreviousCharacter);
            if (_nextButton != null)
                _nextButton.onClick.AddListener(NextCharacter);
            if (_actionButton != null)
                _actionButton.onClick.AddListener(UpgradeSelectedCharacter);
            if (_backButton != null)
                _backButton.onClick.AddListener(BackToGame);
        }

        private void SwitchCharacter(int direction)
        {
            if (_isTransitioning || CharacterCount <= 1 || _selectionRoot == null)
                return;

            _isTransitioning = true;
            SetControlsInteractable(false);
            _selectionRoot.DOKill();
            _selectionGroup?.DOKill();

            var outPosition = _selectionBasePosition + new Vector2(-direction * TransitionDistance, 0f);
            var inPosition = _selectionBasePosition + new Vector2(direction * TransitionDistance, 0f);

            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(_selectionRoot.DOAnchorPos(outPosition, TransitionTime).SetEase(Ease.InBack));
            if (_selectionGroup != null)
                sequence.Join(_selectionGroup.DOFade(0.18f, TransitionTime * 0.75f));
            sequence.Join(_selectionRoot.DOScale(0.88f, TransitionTime * 0.75f));
            sequence.Join(_selectionRoot.DORotate(new Vector3(0f, 0f, direction * 4f), TransitionTime * 0.75f));
            sequence.AppendCallback(() =>
            {
                _currentIndex = WrapIndex(_currentIndex + direction);
                _selectionRoot.anchoredPosition = inPosition;
                _selectionRoot.localScale = Vector3.one * 0.88f;
                _selectionRoot.localEulerAngles = new Vector3(0f, 0f, -direction * 4f);
                if (_selectionGroup != null)
                    _selectionGroup.alpha = 0.18f;
                RefreshView(false, string.Empty);
            });
            sequence.Append(_selectionRoot.DOAnchorPos(_selectionBasePosition, TransitionTime).SetEase(Ease.OutBack));
            if (_selectionGroup != null)
                sequence.Join(_selectionGroup.DOFade(1f, TransitionTime));
            sequence.Join(_selectionRoot.DOScale(1f, TransitionTime));
            sequence.Join(_selectionRoot.DORotate(Vector3.zero, TransitionTime));
            sequence.OnComplete(() =>
            {
                _isTransitioning = false;
                SetControlsInteractable(true);
            });
        }

        private void RefreshView(bool flash, string statusOverride)
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, CharacterCount - 1);

            var unlocked = IsUnlocked(_currentIndex);
            var current = PlayerPrefs.GetInt(CurrentSkinKey, 0) == _currentIndex;
            var price = GetPrice(_currentIndex);
            var coins = PlayerPrefs.GetInt(CoinsKey, 0);

            if (_robotRoot != null)
                _robotRoot.gameObject.SetActive(unlocked);
            if (_lockedCharacter != null)
                _lockedCharacter.gameObject.SetActive(!unlocked);
            if (_lockIcon != null)
                _lockIcon.gameObject.SetActive(!unlocked);

            ApplyTint(GetTint(_currentIndex), unlocked);

            if (_nameText != null)
                _nameText.text = GetString(_names, _currentIndex, $"MODULE-{_currentIndex + 1:00}");
            if (_descriptionText != null)
                _descriptionText.text = GetString(_descriptions, _currentIndex, "Experimental descent unit.");
            if (_balanceText != null)
                _balanceText.text = coins.ToString();
            if (_priceText != null)
                _priceText.text = unlocked ? string.Empty : price > 0 ? price.ToString() : "СКОРО";
            if (_stateText != null)
                _stateText.text = string.IsNullOrEmpty(statusOverride) ? GetStateText(unlocked, current, price) : statusOverride;
            if (_actionText != null)
                _actionText.text = current ? "ВЫБРАН" : unlocked ? "ВЫБРАТЬ" : price > 0 ? "УЛУЧШИТЬ" : "СКОРО";

            if (_actionButton != null)
                _actionButton.interactable = !current && (unlocked || price > 0);

            if (flash && _selectionRoot != null)
                _selectionRoot.DOPunchScale(Vector3.one * 0.08f, 0.28f, 8, 0.7f).SetUpdate(true);
        }

        private string GetStateText(bool unlocked, bool current, int price)
        {
            if (current)
                return "ВЫБРАН ДЛЯ СПУСКА";
            if (unlocked)
                return "ГОТОВ К НАЗНАЧЕНИЮ";
            if (price > 0)
                return "ТРЕБУЕТ РАЗБЛОКИРОВКИ";
            return "СЕКЦИЯ ЗАКРЫТА";
        }

        private void ApplyTint(Color tint, bool unlocked)
        {
            if (_tintedRobotImages != null)
            {
                foreach (var image in _tintedRobotImages)
                {
                    if (image == null)
                        continue;

                    var color = Color.Lerp(Color.white, tint, 0.38f);
                    color.a = unlocked ? 1f : 0.36f;
                    image.color = color;
                }
            }

            if (_leftFire != null)
                _leftFire.color = tint;
            if (_rightFire != null)
                _rightFire.color = tint;
            if (_lockedCharacter != null)
                _lockedCharacter.color = new Color(0.3f, 0.48f, 0.52f, 0.56f);
        }

        private void SetControlsInteractable(bool interactable)
        {
            if (_previousButton != null)
                _previousButton.interactable = interactable;
            if (_nextButton != null)
                _nextButton.interactable = interactable;
            if (_actionButton != null)
            {
                var current = PlayerPrefs.GetInt(CurrentSkinKey, 0) == _currentIndex;
                _actionButton.interactable = interactable && !current && (IsUnlocked(_currentIndex) || GetPrice(_currentIndex) > 0);
            }
            if (_backButton != null)
                _backButton.interactable = interactable;
        }

        private void PulseDenied()
        {
            if (_actionButton == null)
                return;

            _actionButton.transform.DOKill();
            _actionButton.transform
                .DOPunchScale(Vector3.one * 0.12f, 0.28f, 9, 0.7f)
                .SetUpdate(true);
        }

        private bool IsUnlocked(int index)
        {
            return index == 0 || PlayerPrefs.GetInt($"{SkinKeyPrefix}{index}", 0) == 1;
        }

        private int GetPrice(int index)
        {
            if (_prices == null || index < 0 || index >= _prices.Length)
                return 0;
            return _prices[index];
        }

        private Color GetTint(int index)
        {
            if (_skinTints != null && index >= 0 && index < _skinTints.Length)
                return _skinTints[index];

            return _fallbackTints[Mathf.Abs(index) % _fallbackTints.Length];
        }

        private int WrapIndex(int index)
        {
            var count = CharacterCount;
            return ((index % count) + count) % count;
        }

        private static string GetString(string[] values, int index, string fallback)
        {
            if (values == null || index < 0 || index >= values.Length || string.IsNullOrEmpty(values[index]))
                return fallback;
            return values[index];
        }

        private static float Pulse(float time, float speed)
        {
            return (Mathf.Sin(time * speed) + 1f) * 0.5f;
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}
