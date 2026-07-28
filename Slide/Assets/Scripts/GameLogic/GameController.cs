using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using Installers;
// using Plugins;
using Signals;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace GameLogic
{
    public enum GameMode
    {
        Challenge,
        Survival
    }
    
    public class GameController : MonoBehaviour
    {
        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;


        [Inject] private HeroController _heroController;
        [Inject] private ObjectController _objectController;
        [Inject] private UIController _uiController;
        [Inject] private SoInstaller.GameSettings _settings;
        [Inject] private MissionsController _missionsController;
        // [Inject] private FirebaseController _firebase;
        private Vector3 _originalCamPos;

        private ReactiveProperty<int> _points;
        private ReactiveProperty<int> _coins;
        private int _currentLevel = 1;
        private int _lineCounter = 0;

        private GameMode _gameMode;
        private CompositeDisposable _compositeDisposable;
        private SignalBus _signalBus;
        private int _currentRecord;
        private bool _heroIsDie;
        private int _lastCoins;
        private bool _isTutorial;
        private int _sessionLevelsCompleted;
        private int _lastCompletedChallengeLevel;
        private FutureCityEnvironmentController _futureCityEnvironment;
        private ChallengeObjectiveController _challengeObjective;
        private bool _failureFinalized;

        public GameMode Mode
        {
            get { return _gameMode; }
            set
            {
                _gameMode = value;
                _futureCityEnvironment?.Refresh();
            }
        }

        public int LastCoins => _lastCoins;
        public int Coins => _coins.Value;
        public int CurrentLevel => _currentLevel;
        public int Points => _points.Value;
        public int Record => Mathf.Max(_currentRecord, _points.Value);
        public int PlatformsPassed => _lineCounter;
        public int SessionLevelsCompleted => _sessionLevelsCompleted;
        public int LastCompletedChallengeLevel => _lastCompletedChallengeLevel;
        public bool IsVabrate => _uiController.VibrationIsOn;
        public bool IsTutorial => _isTutorial;
        public ChallengeObjectiveController ChallengeObjective => _challengeObjective;
        public ChallengeLevelDefinition CurrentChallengeDefinition =>
            _gameMode == GameMode.Challenge ? ChallengeLevelCatalog.Get(_currentLevel) : null;
        public float CurrentPlayerSpeed => CurrentChallengeDefinition?.PlayerSpeed ?? _settings.speed;
        public bool IsChallengeRunPlaying =>
            _gameMode == GameMode.Challenge && _uiController.IsGame && !_heroIsDie;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        // Use this for initialization
        void Awake ()
        {
            _points = new ReactiveProperty<int>(0);
            _coins = new ReactiveProperty<int>(0);
            
            _currentRecord = PlayerPrefs.GetInt("Record", 0);
            ChallengeProgress.Initialize();
            _currentLevel = ChallengeProgress.SelectedLevel;
            _coins.Value = PlayerPrefs.GetInt("Coins", 0);
            _isTutorial = PlayerPrefs.GetInt("Tutorial", 0) == 0;
            PlayerPrefs.SetInt($"Skin{0}", 1);

            _points.Value = _currentRecord;
            
            SubscribeToTextIfPresent(_points, _uiController.GamePoints);
            SubscribeToTextIfPresent(_points, _uiController.FailPoints);
            SubscribeToTextIfPresent(_points, _uiController.ClearPoints);
            SubscribeToTextIfPresent(_points, _uiController.DeathRecord);
            SubscribeToTextIfPresent(_points.Scan(int.MinValue, Mathf.Max), _uiController.DeathBestRecord);
            SubscribeToTextIfPresent(_points.Scan(int.MinValue, Mathf.Max), _uiController.Record);
            SubscribeToTextIfPresent(_coins, _uiController.GameCoins);
            SubscribeToTextIfPresent(_coins, _uiController.FailCoins);
            SubscribeToTextIfPresent(_coins, _uiController.ClearCoins);
            SubscribeToTextIfPresent(_coins, _uiController.StoreCoins);
            
            _signalBus.Subscribe<TouchLine>(HeroTouchLine);
            _signalBus.Subscribe<TouchBonus>(HeroTouchBonus);
            _signalBus.Subscribe<HeroDie>(CheckRecord);
            _signalBus.Subscribe<ClearAll>(MoveDoors);
            _signalBus.Subscribe<Video>(x => OnVideoWatched(x.isContinue, x.isFinished));
            _signalBus.Subscribe<Perfect>(x => OnPerfect(x.count));
            
            _compositeDisposable = new CompositeDisposable();
#if UNITY_EDITOR
            var clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0) && _uiController.IsGame);
#else
            var clickStream = Observable.EveryUpdate()
                .Where(_ => Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && _uiController.IsGame);
#endif
            clickStream
                .Subscribe(Click())
                .AddTo(_compositeDisposable);
        }

        

        private void Start()
        {
            _points.Value = 0;
            _challengeObjective = GetComponent<ChallengeObjectiveController>();
            if (_challengeObjective == null)
                _challengeObjective = gameObject.AddComponent<ChallengeObjectiveController>();
            _challengeObjective.Initialize(this, _heroController, _objectController, _uiController);
            _futureCityEnvironment = FutureCityEnvironmentController.Create(this);
        }

        private static void SubscribeToTextIfPresent(
            IObservable<int> source,
            Text target)
        {
            if (target != null)
                source.SubscribeToText(target, value => value.ToString());
        }

        private void CheckRecord()
        {
            _heroIsDie = true;
        }

        public void FinalizeFailedAttempt()
        {
            if (_failureFinalized)
                return;

            _failureFinalized = true;
            if (_points.Value > _currentRecord)
            {
                _currentRecord = _points.Value;
                PlayerPrefs.SetInt("Record", _points.Value);
            }

            PlayerPrefs.SetInt("Coins", _coins.Value);
            PlayerPrefs.Save();
        }
        
        
        private void  MoveDoors()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_leftDoor.DOLocalMoveX(_leftDoor.localPosition.x + 0.5f, 0.0f));
            sequence.Join(_rightDoor.DOLocalMoveX(_rightDoor.localPosition.x - 0.5f, 0.0f));
        }
        
        private void OnVideoWatched(bool isContinue, bool isFinished)
        {
            if(!isContinue && isFinished)
                _coins.Value += _lastCoins;
            if (isContinue && isFinished)
                _heroIsDie = false;
        }

        private void OnPerfect(int count)
        {
            if (count != 0)
            {
                var perfectPoints = (_currentLevel + (int) Mathf.Round(count / 10 * _currentLevel));
                _points.Value += perfectPoints;
            }
        }

        private IObserver<long> Click()
        {
            return Observer.Create<long>(_ =>
            {
                _heroController.Jump();
//                _missionsController.Check(_gameMode, MissionTarget.Jump);
            });
        }

        public void PlayGame()
        {
            _futureCityEnvironment?.Refresh();
            _isTutorial = PlayerPrefs.GetInt("Tutorial", 0) == 0;
            _failureFinalized = false;
            
            if (_heroIsDie)
            {
                _heroIsDie = false;
                _points.Value = 0;
                _lastCoins = 0;
            }

            _uiController.CurrentLevel.text = _currentLevel.ToString();
            _uiController.NextLevel.text = Mathf.Min(ChallengeLevelCatalog.LevelCount, _currentLevel + 1).ToString();
            
            _lineCounter = 0;
            if (_gameMode == GameMode.Challenge)
                _challengeObjective.Begin(_currentLevel);
            else
                _challengeObjective.End();
            _heroController.Reset();
            _objectController.GenerateField(_isTutorial);

            var sequence = DOTween.Sequence();
            sequence.Append(_leftDoor.DOLocalMoveX(_leftDoor.localPosition.x - 0.5f, 0.4f));
            sequence.Join(_rightDoor.DOLocalMoveX(_rightDoor.localPosition.x + 0.5f, 0.4f));
            sequence.AppendInterval(0.2f);
            sequence.OnComplete(() =>
            {
                _heroController.Jump();
            });

        }

        private void OnDisable()
        {
            _compositeDisposable?.Dispose();
            
        }

        public void HeroTouchLine()
        {
            _lineCounter++;
            _points.Value += _currentLevel;
            if (_gameMode == GameMode.Challenge)
                _challengeObjective.OnPlatformReached();
        }
        
        private void HeroTouchBonus(TouchBonus touch)
        {
            if (touch?.bonus == null)
                return;

            switch (touch.bonus.Type)
            {
                case BonusType.Coin:
                    _coins.Value++;
                    _lastCoins++;
                    break;
                case BonusType.MissionItem:
                    if (_gameMode == GameMode.Challenge)
                        _challengeObjective.OnMissionItemCollected();
                    break;
            }
        }

        public void CompleteChallengeLevel()
        {
            if (_gameMode != GameMode.Challenge)
                return;

            _sessionLevelsCompleted++;
            var completedLevel = _currentLevel;
            _lastCompletedChallengeLevel = completedLevel;
            _currentLevel = ChallengeProgress.CompleteLevel(completedLevel);
            _missionsController.Check(Mode, MissionTarget.Level);
            PlayerPrefs.SetInt("Coins", _coins.Value);
            PlayerPrefs.Save();
            _heroController.Pause();

            Debug.Log($"Challenge level {completedLevel} completed");
            _uiController.OpenDeathPanel();
        }

        public void FailChallengeObjective()
        {
            if (_gameMode == GameMode.Challenge)
                _heroController.FailChallenge();
        }

        public void AddCoins(int amount)
        {
            _coins.Value += amount;
            PlayerPrefs.SetInt("Coins", _coins.Value);
        }

        public void SpendCoins(int amount)
        {
            _coins.Value = Mathf.Max(0, _coins.Value - Mathf.Max(0, amount));
            PlayerPrefs.SetInt("Coins", _coins.Value);
        }

        public bool TrySpendCoins(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (_coins.Value < amount)
                return false;

            SpendCoins(amount);
            return true;
        }

        public void BeginNewSession()
        {
            _sessionLevelsCompleted = 0;
        }
        
        public void PreContinue()
        {
            _lineCounter = Mathf.Max(0, _lineCounter - 1);
            if (_gameMode == GameMode.Challenge)
                _challengeObjective.PrepareContinue();
            _heroController.PreContinue();
        }

        public void ContinueGame()
        {
            _heroController.ContinueGame();
        }

        public void ContinueFromOffer()
        {
            _heroIsDie = false;
            PreContinue();
            ContinueGame();
        }

        private void OnApplicationQuit()
        {
            var iapTime = PlayerPrefs.GetFloat("IAPTime", 0);
            if (iapTime >= 0)
            {
                iapTime += Time.time;
                PlayerPrefs.SetFloat("IAPTime", iapTime);
            }
                
        }
    }
}
