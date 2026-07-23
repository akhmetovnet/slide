using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class StoreController : MonoBehaviour
{
    private const int OFFSET = 9;

    
    [SerializeField] private GameObject _skinsButton;
    [SerializeField] private Image _skinImage;
    [SerializeField] private Sprite[] _skins;
    [SerializeField] private TMP_Text _skinPrice;
    [SerializeField] private TMP_Text _noAdsPrice;
    [SerializeField] private TMP_Text _allPackPrice;

    [Inject] private UnityStore _unityStore;
    
    private int _currentSkin = 0;
    private List<int> _unboughtSkins;

    private void EnsureUnboughtSkins()
    {
        if (_unboughtSkins == null)
            _unboughtSkins = new List<int>();
    }

    private void Awake()
    {
        EnsureUnboughtSkins();
    }

    private void Start()
    {
        UpdateSkins();
        RefreshPrices();
    }

    public void RefreshPrices()
    {
        EnsureUnboughtSkins();

#if UNITY_ANDROID && RUSTORE_BUILD
        _skinPrice.text = string.Empty;
        _noAdsPrice.text = string.Empty;
        _allPackPrice.text = string.Empty;
#else
        if (_unboughtSkins.Count > 0)
            _skinPrice.text = _unityStore.GetLocalizedPrice($"skin{_unboughtSkins[_currentSkin] + 1}");

        _noAdsPrice.text = _unityStore.GetLocalizedPrice("no_ads");
        _allPackPrice.text = _unityStore.GetLocalizedPrice("all_pack");
#endif
    }

    public void UpdateSkins()
    {
        EnsureUnboughtSkins();
        _unboughtSkins.Clear();

#if UNITY_ANDROID && RUSTORE_BUILD
        _currentSkin = 0;
        _skinsButton.SetActive(false);
        _skinPrice.text = string.Empty;
#else
        _currentSkin = 0;
        for (var i = OFFSET; i < OFFSET + _skins.Length; i++)
        {
            if (PlayerPrefs.GetInt($"Skin{i}", 0) == 0)
                _unboughtSkins.Add(i);
        }
        _currentSkin = 0;

        _skinsButton.SetActive(_unboughtSkins.Count > 0);
        if (_unboughtSkins.Count > 0)
        {
            var index = _unboughtSkins[_currentSkin];
            _skinImage.sprite = _skins[index - OFFSET];
        }
        else
        {
            _skinPrice.text = string.Empty;
        }
#endif
    }

    public void BuySkin()
    {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
        EnsureUnboughtSkins();
        if (_unboughtSkins.Count == 0)
            return;

        _unityStore.BuyProductID($"skin{_unboughtSkins[_currentSkin] + 1}");
#endif
    }
    
    public void BuyNoAds()
    {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
        _unityStore.BuyProductID($"no_ads");       
#endif
    }

    public void BuyAllPack()
    {
#if !(UNITY_ANDROID && RUSTORE_BUILD)
        _unityStore.BuyProductID($"all_pack");
#endif
    }

    public void ChangeSkin(bool isRight)
    {
        EnsureUnboughtSkins();
        if (_unboughtSkins.Count == 0)
            return;

        if (isRight)
        {
            _currentSkin++;
            if (_currentSkin >= _unboughtSkins.Count)
                _currentSkin = 0;

            var index = _unboughtSkins[_currentSkin];
            _skinImage.sprite = _skins[index - OFFSET];
        }
        else
        {
            _currentSkin--;
            if (_currentSkin < 0)
                _currentSkin = _unboughtSkins.Count - 1;

            var index = _unboughtSkins[_currentSkin];
            _skinImage.sprite = _skins[index - OFFSET];
        }
    }
    
}
