using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using GameLogic;
using Installers;
// using Plugins;
using Signals;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private const float FadeTime = 0.75f;
        
        [Header("Texts")]
        [SerializeField] private Image _tapToStart;
        [SerializeField] private Image _title;
        [SerializeField] private TMP_Text _continueTimer;
        [SerializeField] private Text _gamePoints;
        [SerializeField] private Text _failPoints;
        [SerializeField] private Text _clearPoints;
        [SerializeField] private Text _gameCoins;
        [SerializeField] private Text _failCoins;
        [SerializeField] private Text _clearCoins;
        [SerializeField] private Text _storeCoins;
        [SerializeField] private Text _record; 
        [SerializeField] private Text _deathRecord; 
        [SerializeField] private Text _deathBestRecord; 
        [SerializeField] private Text _currentLevel; 
        [SerializeField] private Text _nextLevel; 
        [SerializeField] private Text _perfect; 
        [SerializeField] private TMP_Text _doubleText; 
        [SerializeField] private CanvasGroup _modeGroup;

        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private GameObject _skinsPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _continuePanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _logoPanel;
        [SerializeField] private Transform _buttonsGroup;
        [SerializeField] private Transform _missionsPanel;
        

        
        [Header("Other")]
        [SerializeField] private GameObject _restoreButton;
        [SerializeField] private GameObject _restartButton;
        [SerializeField] private Animator _failAnimator;
        [SerializeField] private Animator _clearAnimator;
        [SerializeField] private Image _slider;
        [SerializeField] private GameObject _sliderParent;
        [SerializeField] private GameObject _doubleButton;
        [SerializeField] private Button _adsContinueButton;
        [SerializeField] private Button _coinsContinueButton;
        [SerializeField] private Image _modeText;
        [SerializeField] private Sprite[] _modeSprites;
        [SerializeField] private Toggle _soundsToggle;
        [SerializeField] private Toggle _vibrationToggle;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Image _storePanelImage;
        [SerializeField] private Sprite[] _storePanelsButtons;
        [SerializeField] private GameObject[] _storePanels;
        [SerializeField] private GameObject _perfectParent;
        [SerializeField] private RectTransform _socialSliderTransform;
        [SerializeField] private Sprite[] _socialSliderSprites;
        [SerializeField] private MissionView _missionView;
        [SerializeField] private GameObject[] _rustoreHiddenObjects;
        [SerializeField] private ContinueOfferView _continueOfferView;
        [SerializeField] private ResultMenuView _resultMenuView;
        
        
        // [Inject] private FirebaseController _firebaseController;
        [Inject] private GameController _gameController;
        [Inject] private HeroController _heroController;
        [Inject] private SoInstaller.GameSettings _settings;
        // [Inject] private AppodealController _appodeal;
        [Inject] private SoundController _soundController;
        [Inject] private UnityStore _unityStore;
        [Inject] private StoreController _storeController;
        [Inject] private ScreenshotController _screenshotController;
        [Inject] private IRewardedAdService _rewardedAdService;

        private int _modeCount;
        private int _halfHeight;
        private SignalBus _signalBus;
        private bool _isGame;
        private bool _soundsIsOn;
        private bool _vibrationIsOn;
        private bool _isSocialSliderOpen;
        private float _startSurvivalTime;
        private int _dies;
        private bool _noAds;
        private bool _isAnimation;
        private float _survivalTime;
        private float _videoTime;
        private int _continueCount;
        private int _consecutiveCoinContinues;
        private bool _continueOfferPending;
        private Transform _settingsTransform;
        private Transform _deathTransform;
        private Transform _continueTransform;
        private Transform _skinsTransform;
        private Text _challengeObjectiveText;
        private static readonly int StartAnimation = Animator.StringToHash("Start");

        public Text GameCoins => _gameCoins;
        public Text FailCoins => _failCoins;
        public Text ClearCoins => _clearCoins;
        public Text StoreCoins => _storeCoins;
        public Text GamePoints => _gamePoints;
        public Text FailPoints => _failPoints;
        public Text ClearPoints => _clearPoints;
        public Text CurrentLevel => _currentLevel;
        public Text NextLevel => _nextLevel;
        public Text DeathRecord => _deathRecord;
        public Text DeathBestRecord => _deathBestRecord;
        public Text Record => _record;
        public bool IsGame => _isGame;
        public bool VibrationIsOn => _vibrationIsOn;
        public bool SoundIsOn => _soundsIsOn;
        public AudioMixer Mixer => _audioMixer;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<HeroDie>(OnHeroDie);
            _signalBus.Subscribe<Video>(x => OnVideoWatched(x.isContinue, x.isFinished));
            _signalBus.Subscribe<Perfect>(x => OnPerfect(x.count));
        }

        void Start ()
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            HideRuStoreUnsupportedUi();
#endif
            _settingsTransform = _settingsPanel.transform.GetChild(0);
            _deathTransform = _resultMenuView != null ? _resultMenuView.transform : _deathPanel.transform.GetChild(0);
            _continueTransform = _continueOfferView != null ? _continueOfferView.transform : _continuePanel.transform.GetChild(0);
            _skinsTransform = _skinsPanel.transform.GetChild(0);

            if (_continueOfferView != null)
                _continueOfferView.Configure(SkipContinue, RequestRewardedContinue, TryCoinContinue);
            if (_resultMenuView != null)
                _resultMenuView.Configure(
                    HandleResultPrimary,
                    OpenSkinsPanel,
                    OpenMissionsFromResult,
                    PurchaseResultProduct);
            
            _soundController.PlayMusic("menu_theme", false);
            
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            _modeCount = Enum.GetNames(typeof(GameMode)).Length;
            _halfHeight = Screen.height / 2;
            _modeText.sprite = _modeSprites[(int)_gameController.Mode];

            _videoTime = Time.time;
            
            _soundsIsOn = PlayerPrefs.GetInt("Sounds", 1) == 1;
            _soundsToggle.isOn = _soundsIsOn;
            
            _vibrationIsOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
            _vibrationToggle.isOn = _vibrationIsOn;

            EnsureChallengeObjectiveText();
            BindMissionMapButton();
            TryAutoStartMission();
        }

        private void BindMissionMapButton()
        {
            if (_buttonsGroup == null)
                return;

            foreach (var button in _buttonsGroup.GetComponentsInChildren<Button>(true))
            {
                if (button.name != "Missions")
                    continue;

                button.onClick.AddListener(OpenMissionMenu);
                break;
            }
        }

        private void TryAutoStartMission()
        {
            if (PlayerPrefs.GetInt(ChallengeProgress.AutoStartKey, 0) != 1)
                return;

            PlayerPrefs.SetInt(ChallengeProgress.AutoStartKey, 0);
            PlayerPrefs.Save();
            _gameController.Mode = GameMode.Challenge;
            _modeText.sprite = _modeSprites[(int)GameMode.Challenge];
            _logoPanel.SetActive(false);

            Observable.NextFrame()
                .Subscribe(_ => PlayGame(true))
                .AddTo(gameObject);
        }

        private void HideRuStoreUnsupportedUi()
        {
            if (_rustoreHiddenObjects == null)
                return;

            foreach (var item in _rustoreHiddenObjects)
            {
                if (item != null)
                    item.SetActive(false);
            }

            _isSocialSliderOpen = false;
        }
        
        private void OnHeroDie()
        {
            _soundController.PlayMusic("menu_theme", true);
            _isGame = false;
            if (_continueOfferPending)
                return;

            _continueOfferPending = true;

            Observable.Timer(TimeSpan.FromSeconds(.5f))
                .Subscribe(_ =>
                {
                    _continueOfferPending = false;
                    if (_gameController.Mode == GameMode.Challenge)
                        OpenContinuePanel();
                    else
                        OpenResultPanel(false);
                })
                .AddTo(gameObject);
        }
        
        private void OnVideoWatched(bool isContinue, bool isFinished)
        {
            
            if(!isFinished) return;
            
            _dies = 0;
            if (isContinue)
            {
                _isGame = true;
                _gamePanel.SetActive(true);
                _continuePanel.SetActive(false);
                if (_continueOfferView != null)
                    _continueOfferView.Close();
                _gameController.PreContinue();
                var sequence = DOTween.Sequence();
                sequence.AppendCallback(() => _continueTimer.text = "3")
                    .AppendInterval(1.0f)
                    .AppendCallback(() => _continueTimer.text = "2")
                    .AppendInterval(1.0f)
                    .AppendCallback(() => _continueTimer.text = "1")
                    .AppendInterval(1.0f)
                    .AppendCallback(() => _continueTimer.text = "GO")
                    .AppendInterval(0.5f)
                    .OnComplete(() =>
                    {
                        _gameController.ContinueGame();
                        _continueTimer.text = string.Empty;
                    });
                // _firebaseController.SimpleLog("ContinueGame");

            }
            else
            {
                if (_doubleButton != null)
                    _doubleButton.SetActive(false);
                // _firebaseController.SimpleLog("DoubleCoins");

            }
        }

        private void OnPerfect(int count)
        {
            _perfectParent.SetActive(count != 0);
            _perfect.text = count.ToString();
        }
        
        public void OpenSkinsPanel()
        {
            _soundController.PlaySound("push_button");

            _skinsTransform.localScale = Vector3.zero;
            _skinsTransform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
            
            _mainPanel.SetActive(false);
            _skinsPanel.SetActive(true);
            _heroController.Hide();
        }
        
        public void OpenContinuePanel()
        {
            if (_continueOfferView != null)
            {
                if (_gameController.Mode != GameMode.Challenge ||
                    (_continueOfferView.State != ContinueOfferState.Hidden &&
                     _continueOfferView.State != ContinueOfferState.Closed))
                    return;

                if (!_continueOfferView.CanShow)
                {
                    Debug.LogError("Continue offer UI is not configured. Opening final result instead.");
                    OpenResultPanel(false);
                    return;
                }

                _gamePanel.SetActive(false);
                _deathPanel.SetActive(false);
                _continuePanel.SetActive(true);
                Time.timeScale = 0f;
                _continueOfferView.Show(
                    _settings.continueOfferDuration,
                    _settings.skipAppearDelay,
                    _settings.skipCloseDuration,
                    _gameController.LastCoins,
                    _gameController.Points,
                    _gameController.Coins,
                    ContinuePrice,
                    CoinContinueAllowedByRule);
                return;
            }

            _continueTransform.localScale = Vector3.zero;
            _continuePanel.GetComponent<Button>().interactable = false;
            _gamePanel.SetActive(false);
            _continuePanel.SetActive(true);
            _restartButton.SetActive(false);
            _continueTransform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _failAnimator.SetTrigger(StartAnimation);
                _continuePanel.GetComponent<Button>().interactable = true;
            });
            Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ => _restartButton.SetActive(true));
            _coinsContinueButton.interactable = (_gameController.Coins >= _settings.coinsToContinue);
            _coinsContinueButton.GetComponentInChildren<Text>().color = _coinsContinueButton.interactable ? Color.white : Color.gray;
            _failPoints.rectTransform.sizeDelta = new Vector2(_failPoints.text.Length*12, 20);
            _failCoins.rectTransform.sizeDelta = new Vector2(_failCoins.text.Length*12, 20);
            
            
            // if (_gameController.LastCoins > 0)
            // {

                // Firebase.Analytics.Parameter[] parameters = { new Firebase.Analytics.Parameter("Count", _gameController.LastCoins)};
                // _firebaseController.LogWithParameters("CoinsBySession", parameters);
            // }

            // if (_gameController.Mode == GameMode.Survival)
            // {
            //     _firebaseController.SimpleIntLog("SurvivalGame", "Time", (int)(Time.time-_survivalTime));
            // }

        }

        public void OpenDeathPanel()
        {
            if (_resultMenuView != null)
            {
                OpenResultPanel(true);
                return;
            }

            _isGame = false;
            
            _deathTransform.localScale = Vector3.zero;
            _deathPanel.GetComponent<Button>().interactable = false;
            _gamePanel.SetActive(false);
            _continuePanel.SetActive(false);
            _deathPanel.SetActive(true);
            _deathTransform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _clearAnimator.SetTrigger(StartAnimation);
                _deathPanel.GetComponent<Button>().interactable = true;
            });
            _clearPoints.rectTransform.sizeDelta = new Vector2(_clearPoints.text.Length*12, 20);
            _clearCoins.rectTransform.sizeDelta = new Vector2(_clearCoins.text.Length*12, 20);
            
            // _doubleButton.SetActive(_appodeal.RewardedVideoIsLoaded && _gameController.LastCoins > 0);
            _doubleText.text = $"+{_gameController.LastCoins} COINS"; 

            // if (_gameController.LastCoins > 0)
            // {
            //     Firebase.Analytics.Parameter[] parameters = { new Firebase.Analytics.Parameter("Count", _gameController.LastCoins)};
            //     _firebaseController.LogWithParameters("CoinsBySession", parameters);
            // }
            //
            // {
            //     Firebase.Analytics.Parameter[] parameters = { new Firebase.Analytics.Parameter("Level", (_gameController.CurrentLevel - 1))};
            //     _firebaseController.LogWithParameters("LevelComplete", parameters);
            // }

        }

        private int ContinuePrice => _settings.coinsToContinue +
                                     _continueCount * Mathf.Max(0, _settings.coinContinuePriceStep);
        private bool CoinContinueAllowedByRule =>
            _consecutiveCoinContinues < Mathf.Max(1, _settings.coinContinuesBeforeRewarded);

        private void SkipContinue()
        {
            _rewardedAdService.Cancel();
            if (_continueOfferView != null)
                _continueOfferView.Close();
            _continueCount = 0;
            _consecutiveCoinContinues = 0;
            _continuePanel.SetActive(false);
            OpenResultPanel(false);
        }

        private void OpenResultPanel(bool isWin)
        {
            _isGame = false;
            _continueOfferPending = false;
            _continueCount = 0;
            _consecutiveCoinContinues = 0;
            _rewardedAdService.Cancel();
            if (_continueOfferView != null)
                _continueOfferView.Close();

            if (!isWin)
                _gameController.FinalizeFailedAttempt();

            if (_resultMenuView == null)
            {
                Time.timeScale = 1f;
                OpenDeathPanel();
                return;
            }

            _gamePanel.SetActive(false);
            _continuePanel.SetActive(false);
            _deathPanel.SetActive(true);
            Time.timeScale = 0f;

            var missionNumber = isWin
                ? Mathf.Max(1, _gameController.LastCompletedChallengeLevel)
                : Mathf.Max(1, _gameController.CurrentLevel);
            var offerCursor = PlayerPrefs.GetInt("RestartOfferCursor", 0);
            var offer = _storeController.GetRestartOffer(offerCursor);
            if (!isWin)
            {
                PlayerPrefs.SetInt("RestartOfferCursor", offerCursor + 1);
                PlayerPrefs.Save();
            }

            var offerPrice = offer == null
                ? string.Empty
                : _unityStore.GetLocalizedPrice(offer.ProductId);
            var offerAvailable = offer != null &&
                                 _unityStore.IsProductAvailable(offer.ProductId);
            var showNoAds = PlayerPrefs.GetInt("NoAds", 0) == 0;
            _resultMenuView.Show(
                isWin,
                _gameController.Mode,
                _gameController.Coins,
                _gameController.Points,
                _gameController.Record,
                missionNumber,
                offer,
                offerPrice,
                offerAvailable,
                showNoAds,
                showNoAds && _unityStore.IsProductAvailable("no_ads"));
        }

        private void HandleResultPrimary(bool isWin)
        {
            _soundController.PlaySound("push_button");
            if (!isWin)
            {
                PlayGame(false);
                return;
            }

            if (_rewardedAdService.IsShowing)
                return;

            _rewardedAdService.Show(() => PlayGame(false));
        }

        private void PurchaseResultProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId) ||
                !_unityStore.IsProductAvailable(productId))
                return;

            _soundController.PlaySound("push_button");
            _unityStore.BuyProductID(productId);
        }

        private void OpenMissionsFromResult()
        {
            OpenMissionMenu();
        }

        public void OpenMissionMenu()
        {
            _soundController.PlaySound("push_button");

            if (Application.CanStreamedLevelBeLoaded("MissionMenu"))
            {
                SceneManager.LoadScene("MissionMenu");
                return;
            }

            OpenMainMenu(false);
        }

        public void FinishSplashScreen()
        {
            _logoPanel.SetActive(false);

            _noAds = PlayerPrefs.GetInt("NoAds", 0) > 0;
            // if(!_noAds)
            //     _appodeal.ShowBanner(true);
        }

        public void OpenSettingsPanel()
        {
            _settingsTransform.localScale = Vector3.zero;
            _settingsPanel.SetActive(true);
            _settingsTransform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
            _restoreButton.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
        }

        public void ChangeSoundsState()
        {
            _soundsIsOn = _soundsToggle.isOn;
            _audioMixer.SetFloat("Master", _soundsIsOn ? 0 : -80);

            PlayerPrefs.SetInt("Sounds", _soundsIsOn ? 1 : 0);
            
            Debug.Log("SOUND: " + PlayerPrefs.GetInt("Sounds", 1));
        }

        public void ChangeVibrationState()
        {
            _vibrationIsOn = _vibrationToggle.isOn;
            
            PlayerPrefs.SetInt("Vibration", _vibrationIsOn ? 1 : 0);
        }

        public void OpenStore()
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            if (!string.IsNullOrEmpty(_settings.rustoreLink))
            {
                Application.OpenURL(_settings.rustoreLink);
            }
#elif UNITY_ANDROID
            Application.OpenURL(_settings.googlePlayLink);
#elif UNITY_IOS
            Application.OpenURL(_settings.appStoreLink);
#endif
        }
        

        public void Back()
        {
            _soundController.PlaySound("push_button");
            
            _skinsPanel.SetActive(false);
            _mainPanel.SetActive(true);
            _heroController.Show();
        }

        public void OpenMainMenu(bool isOpenStore)
        {
            _dies++;
            // if (!_noAds && _appodeal.InterstitialVideoIsLoaded && _dies >= 5)
            // {
            //     _dies = 0;
            //     _appodeal.SHowInterstital();
            // }
            
            
            _soundController.PlaySound("push_button");
            
            _rewardedAdService.Cancel();
            if (_continueOfferView != null)
                _continueOfferView.Close();
            Time.timeScale = 1f;
            _isGame = false;
            _deathPanel.SetActive(false);
            _continuePanel.SetActive(false);
            _mainPanel.SetActive(true);
            ShowButtons();
            _heroController.Show();
            
            _signalBus.Fire<ClearAll>();
            
            if (isOpenStore) OpenSkinsPanel();
            if (_isSocialSliderOpen) OpenCloseSoscialSlider();
        }

        public void ChangeMode(bool isRight)
        {
            _soundController.PlaySound("push_button");
            
            var mode = _gameController.Mode;
            if (isRight)
            {
                mode++;
                if ((int)mode >= _modeCount)
                    mode = 0;
            }
            else
            {
                mode--;
                if (mode < 0)
                    mode = (GameMode)(_modeCount - 1);
            }
    
            _gameController.Mode = mode;
            _modeText.sprite = _modeSprites[(int)_gameController.Mode];
        }

        public void ChangeStorePanels(int index)
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            if (index != 0)
                return;
#endif
            _storePanelImage.sprite = _storePanelsButtons[index];
            foreach (var storePanel in _storePanels)
                storePanel.SetActive(false);
            _storePanels[index].SetActive(true);

            // if(index == 1)
            //     _firebaseController.SimpleLog("OpenStore");

        }
        
        private void HideButtons()
        {
            _isAnimation = true;
            
            _title.transform.DOLocalMoveY(_title.transform.localPosition.y + _halfHeight, FadeTime);
            _title.DOFade(0, FadeTime);
            
            _record.transform.DOLocalMoveY(_record.transform.localPosition.y + _halfHeight, FadeTime);
            _record.DOFade(0, FadeTime);
            
            _modeGroup.DOFade(0, FadeTime/2);

            _tapToStart.DOFade(0, FadeTime/2);

            _buttonsGroup.DOLocalMoveY(_buttonsGroup.localPosition.y - _halfHeight, FadeTime).OnComplete(() => OpenGamePanel(false));
            _missionsPanel.DOLocalMoveY(_missionsPanel.localPosition.y - _halfHeight, FadeTime);
        }

        private void OpenGamePanel(bool isContinue)
        {
            _soundController.PlayMusic("gameplay_theme", false);
            
            _mainPanel.SetActive(false);
            _continuePanel.SetActive(false);
            _gamePanel.SetActive(true);
            
            _sliderParent.SetActive(_gameController.Mode == GameMode.Challenge);
            if (_challengeObjectiveText != null)
                _challengeObjectiveText.gameObject.SetActive(_gameController.Mode == GameMode.Challenge);
            if(!isContinue)
                _slider.fillAmount = 0f;

        }

        private void ShowButtons()
        {
            _title.transform.DOLocalMoveY(_title.transform.localPosition.y - _halfHeight, FadeTime);
            _title.DOFade(1, FadeTime);
            
            _record.transform.DOLocalMoveY(_record.transform.localPosition.y - _halfHeight, FadeTime);
            _record.DOFade(1, FadeTime);
            
            _modeGroup.DOFade(1, FadeTime);

            _tapToStart.DOFade(1, FadeTime);

            _buttonsGroup.DOLocalMoveY(_buttonsGroup.localPosition.y + _halfHeight, FadeTime).OnComplete(() => { _isAnimation = false; });
            _missionsPanel.DOLocalMoveY(_missionsPanel.localPosition.y + _halfHeight, FadeTime);
            _missionView.UpdateView();
        }

        public void PlayGame(bool isMainMenu)
        {
            _soundController.PlaySound("push_button");
            
            if(_isGame || (_isAnimation && isMainMenu)) return;
            
            _rewardedAdService.Cancel();
            if (_continueOfferView != null)
                _continueOfferView.Close();
            _continueOfferPending = false;
            Time.timeScale = 1f;
            _isGame = true;
            _continueCount = 0;
            _consecutiveCoinContinues = 0;
            if (isMainMenu)
                _gameController.BeginNewSession();
            if (isMainMenu)
            {
                HideButtons();
                Observable.Timer(TimeSpan.FromSeconds(FadeTime))
                    .Subscribe(t => _gameController.PlayGame())
                    .AddTo(gameObject);
            }
            else
            {
                _signalBus.Fire<ClearAll>();
                _deathPanel.SetActive(false);
                _continuePanel.SetActive(false);
                OpenGamePanel(false);
                _gameController.PlayGame();
            }

            if (_gameController.Mode == GameMode.Survival)
            {
                // _firebaseController.SimpleLog("PlaySurvival");
                _survivalTime = Time.time;
            }
            // else
            //     _firebaseController.SimpleLog("PlayLevels");
        }

        public void UpdateSlider(int lineCounter)
        {
            var target = _gameController.ChallengeObjective != null
                ? _gameController.ChallengeObjective.DisplayTarget
                : _settings.linesCountInChallenge;
            UpdateSlider(lineCounter, target);
        }

        public void UpdateSlider(int value, int target)
        {
            _slider.fillAmount = target > 0 ? Mathf.Clamp01((float)value / target) : 0f;
        }

        public void SetChallengeObjective(string title, int value, int target)
        {
            EnsureChallengeObjectiveText();
            UpdateSlider(value, target);
            _challengeObjectiveText.text = $"{title}  {value}/{Mathf.Max(1, target)}";
        }

        private void EnsureChallengeObjectiveText()
        {
            if (_challengeObjectiveText != null || _sliderParent == null)
                return;

            var textObject = new GameObject("Challenge Objective", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            textObject.layer = _sliderParent.layer;
            textObject.transform.SetParent(_sliderParent.transform, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            rect.sizeDelta = new Vector2(300f, 30f);

            _challengeObjectiveText = textObject.GetComponent<Text>();
            _challengeObjectiveText.font = _currentLevel.font;
            _challengeObjectiveText.fontSize = 11;
            _challengeObjectiveText.alignment = TextAnchor.MiddleCenter;
            _challengeObjectiveText.color = new Color(0.85f, 0.97f, 1f, 1f);
            _challengeObjectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _challengeObjectiveText.verticalOverflow = VerticalWrapMode.Truncate;

            var outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0.08f, 0.12f, 0.95f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        public void Share()
        {
            _soundController.PlaySound("push_button");
            
            _screenshotController.ShareScreenshot();
        }

        public void VideoCheat(bool isContinue)
        {
            _soundController.PlaySound("push_button");

            if (isContinue)
                RequestRewardedContinue();
        }
        
        public void CoinsCheat(bool isContinue)
        {
            if (isContinue)
                TryCoinContinue();
        }

        private void RequestRewardedContinue()
        {
            if (_continueOfferView == null ||
                _continueOfferView.State != ContinueOfferState.ContinueProcessing ||
                _rewardedAdService.IsShowing)
                return;

            _soundController.PlaySound("push_button");
            _rewardedAdService.Show(() =>
            {
                _consecutiveCoinContinues = 0;
                ResumeFromContinue();
            });
        }

        private bool TryCoinContinue()
        {
            if (_continueOfferView == null ||
                (_continueOfferView.State != ContinueOfferState.Countdown &&
                 _continueOfferView.State != ContinueOfferState.SkipAvailable))
                return false;

            if (!CoinContinueAllowedByRule)
            {
                _continueOfferView.SetStatus("ИСПОЛЬЗУЙТЕ РЕКЛАМУ");
                return false;
            }

            _soundController.PlaySound("push_button");
            if (!_gameController.TrySpendCoins(ContinuePrice))
            {
                _continueOfferView.SetStatus("НЕДОСТАТОЧНО МОНЕТ");
                return false;
            }

            _continueCount++;
            _consecutiveCoinContinues++;
            _continueOfferView.SetBalance(_gameController.Coins);
            Observable.NextFrame().Subscribe(_ => ResumeFromContinue()).AddTo(gameObject);
            return true;
        }

        private void ResumeFromContinue()
        {
            _rewardedAdService.Cancel();
            _continueOfferPending = false;
            _continueOfferView.Close();
            _continuePanel.SetActive(false);
            Time.timeScale = 1f;
            _isGame = true;
            _gamePanel.SetActive(true);
            _gameController.ContinueFromOffer();
        }

        public void OpenCloseSoscialSlider()
        {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
            if (_isSocialSliderOpen)
                _socialSliderTransform.DOAnchorPosX(0, .5f).OnComplete(delegate { _socialSliderTransform.GetComponent<Image>().sprite = _socialSliderSprites[0]; });
            else
                _socialSliderTransform.DOAnchorPosX(-250, 0.5f).OnComplete(delegate { _socialSliderTransform.GetComponent<Image>().sprite = _socialSliderSprites[1]; });

            _isSocialSliderOpen = !_isSocialSliderOpen;
#endif
        }

        public void OpenSocianNetwork(int index)
        {
            switch (index)
            {
                case 0:
                    Application.OpenURL("https://vk.com/public212469191");
                    // _firebaseController.SimpleLog("Facebook");
                    break;
                case 1:
                    Application.OpenURL("https://discord.gg/3hxUW96mFZ");
                    // _firebaseController.SimpleLog("Facebook");
                    break;
                case 2:
                    Application.OpenURL("https://strazed.com");
                    // _firebaseController.SimpleLog("Portal");
                    break;
            }
        }

        public void SetNoAds()
        {
            _noAds = true;
            PlayerPrefs.SetInt("NoAds", 1);
            PlayerPrefs.Save();
            _resultMenuView?.SetNoAdsEntitled();
            // _appodeal.ShowBanner(false);
        }

        public void BuyNoAds()
        {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
            _soundController.PlaySound("push_button");
            _unityStore.BuyProductID($"no_ads");  
#endif
        }

        public void Restore()
        {
            _soundController.PlaySound("push_button");
            _unityStore.RestorePurchases();
        }

        private void OnDestroy()
        {
            _rewardedAdService?.Cancel();
        }

        public void RateUs()
        {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
            _soundController.PlaySound("push_button");
#if UNITY_ANDROID
            Application.OpenURL ("market://details?id=" + Application.productName);
#else
            Application.OpenURL("itms-apps://itunes.apple.com/app/id1460365903");
#endif
#endif
        }
    }
}
