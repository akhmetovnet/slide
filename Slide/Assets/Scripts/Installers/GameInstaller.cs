using GameLogic;
// using Plugins;
using Signals;
using UI;
using UnityEngine;
using Zenject;

namespace Installers
{
    
    public class GameInstaller : MonoInstaller
    {
        
        [SerializeField] private HeroController _heroController;
        [SerializeField] private GameController _gameController;
        [SerializeField] private ObjectController _objectController;
        [SerializeField] private WallController _wallController;
        [SerializeField] private UIController _uiController;
        [SerializeField] private MissionsController _missionsController;
        [SerializeField] private SoundController _soundController;
        [SerializeField] private StoreController _storeController;
        [SerializeField] private ScreenshotController _screenshotController;
        
        
        public override void InstallBindings()
        {
            Container.BindInstance(_uiController).AsSingle();
            Container.BindInstance(_gameController).AsSingle();
            Container.BindInstance(_heroController).AsSingle();
            Container.BindInstance(_objectController).AsSingle();
            Container.BindInstance(_wallController).AsSingle();
            Container.BindInstance(_missionsController).AsSingle();
            Container.BindInstance(_soundController).AsSingle();
            Container.BindInstance(_screenshotController).AsSingle();
            Container.Bind<IRewardedAdService>()
                .To<RewardedAdStub>()
                .FromNewComponentOn(_uiController.gameObject)
                .AsSingle();
            // Container.Bind<FirebaseController>().AsSingle();
            Container.BindInstance(_storeController).AsSingle();
            // Container.Bind<AppodealController>().AsSingle();
            Container.Bind<UnityStore>().AsSingle();

            Container.BindMemoryPool<LineController, LinePool.LPool>()
                .WithFixedSize(_objectController.Settings[0].count)
                .FromComponentInNewPrefab(_objectController.Settings[0].prefab)
                .UnderTransform(_objectController.Settings[0].parent);
            Container.Bind<LinePool>().AsSingle();
            
            Container.BindMemoryPool<ThornController, ThornPool.TPool>()
                .WithFixedSize(_objectController.Settings[1].count)
                .FromComponentInNewPrefab(_objectController.Settings[1].prefab)
                .UnderTransform(_objectController.Settings[1].parent);
            Container.Bind<ThornPool>().AsSingle();
            
            Container.BindMemoryPool<BonusController, BonusPool.BPool>()
                .WithFixedSize(_objectController.Settings[2].count)
                .FromComponentInNewPrefab(_objectController.Settings[2].prefab)
                .UnderTransform(_objectController.Settings[2].parent);
            Container.Bind<BonusPool>().AsSingle();
            
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<ClearAll>();
            Container.DeclareSignal<HeroDie>();
            Container.DeclareSignal<LineIsOut>();
            Container.DeclareSignal<ThornIsOut>();
            Container.DeclareSignal<BonusIsOut>();
            Container.DeclareSignal<TouchLine>();
            Container.DeclareSignal<TouchBonus>();
            Container.DeclareSignal<Video>();
            Container.DeclareSignal<Perfect>();
            
        }
    }
}
