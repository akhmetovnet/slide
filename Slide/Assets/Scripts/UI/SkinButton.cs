using System.Collections;
using System.Collections.Generic;
using GameLogic;
using Installers;
// using Plugins;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SkinButton : MonoBehaviour
{
    [SerializeField] private Sprite _currentSkinSprite;
    [SerializeField] private Sprite _availableSkinSprite;
    [SerializeField] private Sprite _unavailableSkinSrite;
    [SerializeField] private int _index;
    [SerializeField] private Text _price;
    [SerializeField] private GameObject _shop;
    [SerializeField] private Transform _parent;
    [SerializeField] private GameObject _coinImage;

    // [Inject] private FirebaseController _firebaseController;
    [Inject] private HeroController _hero;
    [Inject] private GameController _gameController;
    [Inject] private UIController _uiController;
    [Inject] private SoInstaller.GameSettings _gameSettings;

    private bool _isAvailable;

    private void OnEnable()
    {
        UpdateSkinInfo();
        
    }

    public void UpdateSkinInfo()
    {
        _isAvailable = PlayerPrefs.GetInt($"Skin{_index}", 0) == 1;

        GetComponent<Image>().sprite = _isAvailable ?_availableSkinSprite : _unavailableSkinSrite;
        if(_isAvailable)
        GetComponent<Image>().sprite = PlayerPrefs.GetInt($"CurrentSkin", 0) == _index ? _currentSkinSprite : _availableSkinSprite;
        _coinImage.SetActive(!_isAvailable && _gameSettings.skins[_index].price > 0 );
        GetComponentInChildren<Text>(true).gameObject.SetActive(!_isAvailable && _gameSettings.skins[_index].price > 0 );
        _price.text = _gameSettings.skins[_index].price.ToString();
        if(_shop != null)
#if UNITY_ANDROID && RUSTORE_BUILD
            _shop.SetActive(false);
#else
            _shop.SetActive(!_isAvailable);
#endif
    }

    public void ClickSkin()
    {
        if (_isAvailable)
        {
            _hero.SetSkin(_index);
            
            PlayerPrefs.SetInt($"CurrentSkin", _index);
            var skinsButtons = _parent.GetComponentsInChildren<SkinButton>();
            for (var i = 0; i < skinsButtons.Length; i++)
                skinsButtons[i].UpdateSkinInfo();
        }
        else if ( _gameSettings.skins[_index].price > 0 && _gameSettings.skins[_index].price <= _gameController.Coins)
        {
            _gameController.SpendCoins(_gameSettings.skins[_index].price);
            PlayerPrefs.SetInt($"Skin{_index}", 1);
            UpdateSkinInfo();
            
            // _firebaseController.SimpleLog("BuyNewSkin");
            // _firebaseController.SimpleIntLog("BuySkin", "Index", _index);
        }
        else if (_gameSettings.skins[_index].price < 0 || _gameSettings.skins[_index].price > _gameController.Coins)
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            return;
#else
            _uiController.ChangeStorePanels(1);
#endif
        }

        
            
    }
}
