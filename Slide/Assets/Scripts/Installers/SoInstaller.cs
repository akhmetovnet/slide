using System;
using GameLogic;
using UnityEngine;
using Zenject;

namespace Installers
{
    public enum MissionTarget
    {
        Jump,
        Die,
        Coin,
        Level,
        Shield,
        Accelerate,
        PerfectJump,
        Platforms
    }
    
    public enum MissionType
    {
        Game,
        Total
    }
    
    [CreateAssetMenu(fileName = "SoInstaller", menuName = "Slide/Create SoInstaller")]
    public class SoInstaller : ScriptableObjectInstaller
    {
        public GameSettings gameSettings;
            
        [Serializable]
        public class GameSettings
        {
            public float speed;
            public int linesCountInChallenge;
            public int linesCountInSurvival;
            public Vector2 offset;
            public float fieldWidht;
            public string googlePlayLink;
            public string rustoreLink;
            public string appStoreLink;
            public int coinsToContinue;
            public SkinSettings[] skins;
            public BonusSettings bonusSettings;
            public ThornSettings thornSettings;
            public MissionSettings[] missionSettings;
        }

        [Serializable]
        public class SkinSettings
        {
            public int price;
        }
        
        [Serializable]
        public class BonusSettings
        {
            public int accelerationLines;
            public int shield;
            public int accelerationMin;
            public int accelerationMax;
            public int accelerationMinSurvival;
            public int accelerationMaxSurvival;

        }
        
        [Serializable]
        public class ThornSettings
        {
            public float speed;
            public float offTime;
            public float onTime;

        }
        
        [Serializable]
        public class MissionSettings
        {
            public MissionTarget target;
            public MissionType type;
            public bool challenge;
            public bool survival;
            public int count;
            public int reward;
            public string title;

        }
        
        public override void InstallBindings()
        {
            Application.targetFrameRate = 60;
            
            Container.BindInstance(gameSettings).IfNotBound();
        }
    }
}
