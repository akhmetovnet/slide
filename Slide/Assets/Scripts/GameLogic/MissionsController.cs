using System;
using System.Collections.Generic;
using Installers;
// using Plugins;
using UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace GameLogic
{
    public class MissionsController : MonoBehaviour
    {
        [Inject] private SoInstaller.GameSettings _settings;
        // [Inject] private FirebaseController _firebaseController;
    
        [SerializeField] private MissionView _missionsView;

        private SoInstaller.MissionSettings[] _missions;
        private int _currentMission;
        private int _missionValue;
        private List<MissionView> _views;
        private SignalBus _signalBus;
        private CompositeDisposable _compositeDisposable;

        public bool HasActiveMission => _missions != null && _currentMission >= 0 && _currentMission < _missions.Length;
        public int CurrentMissionNumber => HasActiveMission ? _currentMission + 1 : _missions?.Length ?? 0;
        
    
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _compositeDisposable = new CompositeDisposable();
            
            _currentMission = PlayerPrefs.GetInt("Mission", 0);
            _missionValue = PlayerPrefs.GetInt("MissionValue", 0);
            Debug.Log("MissionValue0: " + _missionValue + " CurrentMission: " + _currentMission);
        
            _missions = _settings.missionSettings;
        
            if (_currentMission < _missions.Length)
                _missionsView.Init(_missions[_currentMission].title, (float) _missionValue / _missions[_currentMission].count);
            else
                _missionsView.Disable();
        }

        public void Check(GameMode mode, MissionTarget target)
        {
            if (!HasActiveMission)
                return;

            if ((!_missions[_currentMission].survival || mode != GameMode.Survival) && (!_missions[_currentMission].challenge || mode != GameMode.Challenge)) return;
            if(_missions[_currentMission].target != target) return;
            
            _missionValue++;
            PlayerPrefs.SetInt("MissionValue", _missionValue);

        }

        public int GetReward()
        {
            if (!HasActiveMission)
                return 0;

            // _firebaseController.SimpleStringLog("Mission", "Completed", _missions[_currentMission].title);
            
            var reward = _missions[_currentMission].reward;
            _missionValue = 0;
            
            _currentMission++;
            if (_currentMission < _missions.Length)
                _missionsView.Init(_missions[_currentMission].title, 0.0f);
            else
                _missionsView.Disable();
            
            PlayerPrefs.SetInt("Mission", _currentMission);
            PlayerPrefs.SetInt("MissionValue", 0);
            
            return reward;
        }

        public float GetValue()
        {
            if (!HasActiveMission || _missions[_currentMission].count <= 0)
                return 0f;

            return (float) _missionValue / _missions[_currentMission].count;
        }
    }
}
