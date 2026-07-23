using System;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResultMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private TMP_Text _missionText;
        [SerializeField] private TMP_Text _primaryButtonText;
        [SerializeField] private Button _primaryButton;
        [SerializeField] private Button _charactersButton;
        [SerializeField] private Button _missionsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private GameObject _leaderboardRoot;

        public void Configure(
            Action primaryAction,
            Action charactersAction,
            Action missionsAction,
            Action mainMenuAction)
        {
            Bind(_primaryButton, primaryAction);
            Bind(_charactersButton, charactersAction);
            Bind(_missionsButton, missionsAction);
            Bind(_mainMenuButton, mainMenuAction);
        }

        public void Show(
            bool isWin,
            GameMode mode,
            int score,
            int record,
            int sessionLevels,
            int missionNumber)
        {
            gameObject.SetActive(true);

            _titleText.text = isWin ? "УРОВЕНЬ ПРОЙДЕН" : "СПУСК ПРЕРВАН";
            _primaryButtonText.text = isWin ? "СЛЕДУЮЩИЙ" : "ПОВТОРИТЬ";
            _statsText.text = $"СЧЕТ  {score}\nРЕКОРД  {record}\nУРОВНЕЙ ЗА СЕССИЮ  {sessionLevels}";

            var isChallenge = mode == GameMode.Challenge;
            _missionText.gameObject.SetActive(isChallenge);
            if (isChallenge)
                _missionText.text = $"МИССИЯ {missionNumber} - {(isWin ? "ПРОЙДЕНА" : "ПРОВАЛЕНА")}";

            if (_leaderboardRoot != null)
                _leaderboardRoot.SetActive(mode == GameMode.Survival);
        }

        private static void Bind(Button button, Action action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }
    }
}
