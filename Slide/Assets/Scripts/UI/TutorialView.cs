using UnityEngine;

namespace UI
{
    public class TutorialView : MonoBehaviour
    {
        [SerializeField] private GameObject[] _view;
        [SerializeField] private RectTransform _sprite;

        private int _currentStep;

        public int Step => _currentStep;
        public void OpenView(int index)
        {
            _view[index].SetActive(true);
            _currentStep++;
            if (_currentStep >= _view.Length)
                PlayerPrefs.SetInt("Tutorial", 1);
            Time.timeScale = 0.0f;
        }

        public void CloseAll()
        {
            for (var i = 0; i < _view.Length; i++)
                _view[i].SetActive(false);
            
            Time.timeScale = 1.0f;
        }

        public void SetSprite()
        {
            if((float)Screen.width/Screen.height > 0.7)
                _sprite.localPosition = new Vector3(_sprite.localPosition.x-30, _sprite.localPosition.y);
        }
    }
}
