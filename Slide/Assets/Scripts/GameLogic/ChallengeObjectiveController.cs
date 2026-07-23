using UI;
using UnityEngine;

namespace GameLogic
{
    public sealed class ChallengeObjectiveController : MonoBehaviour
    {
        private GameController _gameController;
        private HeroController _heroController;
        private ObjectController _objectController;
        private UIController _uiController;
        private ChallengeRivalController _rivalView;
        private float _rivalProgress;
        private float _elapsed;
        private bool _completed;
        private bool _failed;

        public ChallengeLevelDefinition Definition { get; private set; }
        public int PlatformProgress { get; private set; }
        public int ObjectiveProgress { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFinished => _completed || _failed;

        public int DisplayTarget => Definition == null
            ? 1
            : Definition.Objective == ChallengeObjectiveType.CatchCriminal
                ? Mathf.CeilToInt(Definition.RivalStartLead)
                : Mathf.Max(1, Definition.TargetCount);

        public int DisplayProgress
        {
            get
            {
                if (Definition == null)
                    return 0;
                if (Definition.Objective != ChallengeObjectiveType.CatchCriminal)
                    return ObjectiveProgress;

                var remaining = Mathf.Max(0f, _rivalProgress - PlatformProgress);
                return Mathf.Clamp(
                    Mathf.CeilToInt(Definition.RivalStartLead - remaining),
                    0,
                    DisplayTarget);
            }
        }

        public void Initialize(
            GameController gameController,
            HeroController heroController,
            ObjectController objectController,
            UIController uiController)
        {
            _gameController = gameController;
            _heroController = heroController;
            _objectController = objectController;
            _uiController = uiController;
            _rivalView = ChallengeRivalController.Create(heroController);
        }

        public void Begin(int level)
        {
            Definition = ChallengeLevelCatalog.Get(level);
            PlatformProgress = 0;
            ObjectiveProgress = 0;
            _rivalProgress = Definition.RivalStartLead;
            _elapsed = 0f;
            _completed = false;
            _failed = false;
            IsActive = true;

            if (UsesRival())
                _rivalView.Show(Definition.Objective);
            else
                _rivalView.Hide();

            RefreshView();
        }

        public void End()
        {
            IsActive = false;
            _rivalView?.Hide();
        }

        public void OnPlatformReached(int amount = 1)
        {
            if (!CanProgress())
                return;

            amount = Mathf.Max(1, amount);
            PlatformProgress += amount;

            if (Definition.Objective == ChallengeObjectiveType.ReachPlatforms ||
                Definition.Objective == ChallengeObjectiveType.RaceBot)
            {
                ObjectiveProgress = Mathf.Min(Definition.TargetCount, ObjectiveProgress + amount);
                if (ObjectiveProgress >= Definition.TargetCount)
                {
                    Complete();
                    return;
                }
            }
            else if (Definition.Objective == ChallengeObjectiveType.CatchCriminal &&
                     _elapsed > 0.5f &&
                     PlatformProgress + Definition.CaptureDistance >= _rivalProgress)
            {
                ObjectiveProgress = DisplayTarget;
                Complete();
                return;
            }

            RefreshView();
        }

        public void OnMissionItemCollected()
        {
            if (!CanProgress() || Definition.Objective != ChallengeObjectiveType.CollectItems)
                return;

            ObjectiveProgress = Mathf.Min(Definition.TargetCount, ObjectiveProgress + 1);
            if (ObjectiveProgress >= Definition.TargetCount)
            {
                Complete();
                return;
            }

            RefreshView();
        }

        public void RewindPlatform()
        {
            if (!IsActive || IsFinished)
                return;

            PlatformProgress = Mathf.Max(0, PlatformProgress - 1);
            if (Definition.Objective == ChallengeObjectiveType.ReachPlatforms ||
                Definition.Objective == ChallengeObjectiveType.RaceBot)
                ObjectiveProgress = Mathf.Max(0, ObjectiveProgress - 1);
            RefreshView();
        }

        public void PrepareContinue()
        {
            if (!IsActive || _completed || Definition == null)
                return;

            if (_failed)
            {
                _failed = false;
                if (Definition.Objective == ChallengeObjectiveType.RaceBot)
                    _rivalProgress = Mathf.Min(_rivalProgress, Definition.TargetCount - 1.5f);
                else if (Definition.Objective == ChallengeObjectiveType.CatchCriminal)
                    _rivalProgress = Mathf.Min(
                        _rivalProgress,
                        PlatformProgress + Definition.RivalEscapeLead - 2f);
            }

            if (UsesRival())
                _rivalView.Show(Definition.Objective);
            RewindPlatform();
        }

        private void Update()
        {
            if (!CanProgress() || !UsesRival() || !_gameController.IsChallengeRunPlaying)
                return;

            _elapsed += Time.deltaTime;
            var pacing = 0.94f + Mathf.Sin(_elapsed * 1.7f) * 0.06f;
            _rivalProgress += Definition.RivalPlatformsPerSecond * pacing * Time.deltaTime;
            _rivalView.SetProgress(PlatformProgress, _rivalProgress);

            if (Definition.Objective == ChallengeObjectiveType.RaceBot &&
                _rivalProgress >= Definition.TargetCount)
            {
                Fail();
                return;
            }

            if (Definition.Objective == ChallengeObjectiveType.CatchCriminal)
            {
                _objectController.ConsumeBonusesBefore(Mathf.FloorToInt(_rivalProgress));
                if (_rivalProgress - PlatformProgress >= Definition.RivalEscapeLead)
                {
                    Fail();
                    return;
                }
            }

            RefreshView();
        }

        private bool UsesRival()
        {
            return Definition != null &&
                   (Definition.Objective == ChallengeObjectiveType.RaceBot ||
                    Definition.Objective == ChallengeObjectiveType.CatchCriminal);
        }

        private bool CanProgress()
        {
            return IsActive && Definition != null && !_completed && !_failed;
        }

        private void Complete()
        {
            if (!CanProgress())
                return;

            _completed = true;
            RefreshView();
            _rivalView.Hide();
            _gameController.CompleteChallengeLevel();
        }

        private void Fail()
        {
            if (!CanProgress())
                return;

            _failed = true;
            _rivalView.Hide();
            _gameController.FailChallengeObjective();
        }

        private void RefreshView()
        {
            if (Definition == null)
                return;

            _uiController.SetChallengeObjective(
                Definition.GetTitle(),
                DisplayProgress,
                DisplayTarget);
        }

        private void OnDestroy()
        {
            if (_rivalView != null)
                Destroy(_rivalView.gameObject);
        }
    }
}
