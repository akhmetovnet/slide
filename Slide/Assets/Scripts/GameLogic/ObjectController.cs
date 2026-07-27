using System;
using System.Collections.Generic;
using System.Linq;
using Installers;
using Signals;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class ObjectController : MonoBehaviour, IDisposable
    {
        [SerializeField] private PoolSettings[] _poolSettings;

        [Inject] private WallController _wallController;
        [Inject] private GameController _gameController;
        [Inject] private LinePool _linePool;
        [Inject] private ThornPool _thornPool;
        [Inject] private BonusPool _bonusPool;
        [Inject] private SoInstaller.GameSettings _settings;

        private List<GameObject> lines;
        private List<GameObject> thorns;
    
        private bool isRight;
        private float distanceCoin;
        private int _index;
        private int _accelerateIndex;
        private SignalBus _signalBus;
        private LineController _pastLine;
        private bool _isUseBonus;
        private readonly HashSet<BonusController> _activeBonuses = new HashSet<BonusController>();
        private int _linesWithoutMissionItem;

        public PoolSettings[] Settings
        {
            get { return _poolSettings; }
        }
        
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;

            _signalBus.Subscribe<ThornIsOut>(x => ReturnThorn(x.thorn));
            _signalBus.Subscribe<LineIsOut>(x => ReturnLine(x.line, x.isForce));
            _signalBus.Subscribe<BonusIsOut>(x => ReturnBonus(x.bonus));
            _signalBus.Subscribe<TouchBonus>(x => ReturnBonus(x.bonus));
            _signalBus.Subscribe<ClearAll>(x => HeroDie());
        }

        private void HeroDie()
        {
            _wallController.Move(true, 0.0f);
            _index = 0;
            _linesWithoutMissionItem = 0;
        }


        public void GenerateField(bool isTutorial)
        {
            _pastLine = null;
            _isUseBonus = !_isUseBonus;
            _wallController.Move(false, 0.5f);
            var configuredCount = _gameController.Mode == GameMode.Challenge
                ? _settings.linesCountInChallenge
                : _settings.linesCountInSurvival;
            var count = Mathf.Min(configuredCount, _linePool.Count());
            if(_gameController.Mode == GameMode.Survival)
                _accelerateIndex = Random.Range(_settings.bonusSettings.accelerationMinSurvival, _settings.bonusSettings.accelerationMaxSurvival);
            else
                _accelerateIndex = _isUseBonus ? Random.Range(_settings.bonusSettings.accelerationMin, _settings.bonusSettings.accelerationMax) : _settings.linesCountInChallenge + 1;
            
            for (int i = 0; i < count; i++)
            {
                var isFirst = i == 0;
                var line = _linePool.GetLine();
                if (line == null)
                    break;
                line.SetPositionAndRotation(_index, false);
                if (_pastLine != null) _pastLine.AddNextLine(line);
                _pastLine = line;

                if (!isFirst || (isTutorial && isFirst))
                    PopulateLine(line, _index, isTutorial);

                _index++;
            }
        }

        private void ReturnLine(LineController line, bool isForce)
        {
            var recycle = !isForce &&
                          (_gameController.Mode == GameMode.Survival ||
                           (_gameController.Mode == GameMode.Challenge &&
                            _gameController.ChallengeObjective != null &&
                            _gameController.ChallengeObjective.IsActive &&
                            !_gameController.ChallengeObjective.IsFinished));
            if (!recycle)
            {
                _linePool.ReturnLine(line);
            }
            else
            {
                line.SetPositionAndRotation(_index, false);
                if(_pastLine != null) _pastLine.AddNextLine(line);
                _pastLine = line;
                
                PopulateLine(line, _index, false);

                _index++;
            }
            
        }
        
        private void ReturnThorn(ThornController thorn)
        {
            _thornPool.ReturnThorn(thorn);
        }
        
        private void ReturnBonus(BonusController bonus)
        {
            if (bonus == null || !_activeBonuses.Remove(bonus))
                return;
            if (bonus.Type == BonusType.Acceleration && _gameController.Mode == GameMode.Survival)
                _accelerateIndex = Random.Range(_index + _settings.bonusSettings.accelerationMinSurvival, _index + _settings.bonusSettings.accelerationMaxSurvival);
            _bonusPool.ReturnBonus(bonus);
        }

        public void ConsumeBonusesBefore(int lineIndex)
        {
            if (_activeBonuses.Count == 0)
                return;

            var consumed = _activeBonuses
                .Where(x => x != null && x.LineIndex >= 0 && x.LineIndex <= lineIndex)
                .ToArray();
            foreach (var bonus in consumed)
                ReturnBonus(bonus);
        }

        private void PopulateLine(LineController line, int index, bool isTutorial)
        {
            var challenge = _gameController.CurrentChallengeDefinition;
            var spawnHazard = _gameController.Mode == GameMode.Survival ||
                              challenge == null ||
                              Random.value < challenge.HazardChance;
            if (spawnHazard)
            {
                var thorn = _thornPool.GetThorn();
                if (thorn != null)
                {
                    thorn.Init(index, isTutorial, line.Angle);
                    line.AddThorn(thorn);
                }
            }

            var bonus = _bonusPool.GetBonus();
            if (bonus == null)
                return;

            var bonusType = GetBonusType(index, challenge);
            if (bonusType == null)
            {
                _bonusPool.ReturnBonus(bonus);
                return;
            }

            var missionItemVariant = bonusType.Value == BonusType.MissionItem
                ? Random.Range(0, ChallengeAssetCatalog.MissionItemCount)
                : 0;
            bonus.Init(bonusType.Value, missionItemVariant);
            var positionX = Random.Range(-_settings.fieldWidht, _settings.fieldWidht);
            if (bonus.Type == BonusType.Acceleration)
            {
                positionX = _settings.fieldWidht + .2f;
                positionX *= Random.value > .5f ? 1 : -1;
            }
            bonus.SetPosition(positionX, line.Angle, index);
            _activeBonuses.Add(bonus);
        }

        private BonusType? GetBonusType(int index, ChallengeLevelDefinition challenge)
        {
            if (_gameController.Mode == GameMode.Challenge &&
                challenge != null &&
                challenge.Objective == ChallengeObjectiveType.CollectItems)
            {
                var shouldSpawn = Random.value <= challenge.MissionItemChance || _linesWithoutMissionItem >= 1;
                _linesWithoutMissionItem = shouldSpawn ? 0 : _linesWithoutMissionItem + 1;
                return shouldSpawn ? BonusType.MissionItem : (BonusType?)null;
            }

            return _accelerateIndex == index ? BonusType.Acceleration : BonusType.Coin;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ThornIsOut>(s => ReturnThorn(s.thorn));
            _signalBus.Unsubscribe<LineIsOut>(s => ReturnLine(s.line, s.isForce));
            _signalBus.Unsubscribe<ClearAll>(s => HeroDie());
        }

        public void RemoveAccelerationThorn(LineController line, int linesToDrop)
        {
            for (var i = 0; i < linesToDrop; i++)
                line = line.GetNextLine();

            line.RemoveThorn();
        }
    }
    
    [Serializable]
    public class PoolSettings
    {
        public int count;
        public GameObject prefab;
        public Transform parent;
    }
}
