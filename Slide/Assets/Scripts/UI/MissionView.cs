using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class MissionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _slider;
        [SerializeField] private GameObject _missionsPanel;
        [SerializeField] private GameObject _rewardButton;

        [Inject] private MissionsController _missionsController;
        [Inject] private GameController _gameController;

        public void Init(string title, float value)
        {
            _title.text = title;
            _slider.fillAmount = value;
            _rewardButton.SetActive(value >= 1);
        }

        public void GetReward()
        {
            _gameController.AddCoins(_missionsController.GetReward());
        }

        public void Disable()
        {
            _missionsPanel.SetActive(false);
        }

        public void UpdateView()
        {
            var value = _missionsController.GetValue();
            _slider.fillAmount = value;
            _rewardButton.SetActive(value >= 1);
        }
    }
}
