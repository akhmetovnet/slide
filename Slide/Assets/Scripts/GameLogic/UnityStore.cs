using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameLogic;
// using Plugins;
using UI;
using UnityEngine;
#if USE_UNITY_IAP
using UnityEngine.Purchasing;
#endif
using Zenject;

#if USE_UNITY_IAP
public class UnityStore : IStoreListener
{
    
    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    [Inject] private UIController _uiController;
    [Inject] private StoreController _storeController;
    // [Inject] private FirebaseController _firebaseController;
    
    [Inject]
    private void Construct()
    {
#if UNITY_ANDROID && RUSTORE_BUILD
        _storeController.UpdateSkins();
        _storeController.RefreshPrices();
        return;
#endif
        if (!IsInitialized())
        {
            InitializePurchasing();
            return;
        }

        _storeController.UpdateSkins();
        _storeController.RefreshPrices();
    }
    
    public void InitializePurchasing() 
    {
#if UNITY_ANDROID && RUSTORE_BUILD
        Debug.Log("Purchasing is disabled for RuStore build.");
        return;
#endif
        if (IsInitialized())
        {
            return;
        }
            
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
        builder.AddProduct("skin10", ProductType.NonConsumable);
        builder.AddProduct("skin11", ProductType.NonConsumable);
        builder.AddProduct("skin12", ProductType.NonConsumable);
        builder.AddProduct("no_ads", ProductType.NonConsumable);
        builder.AddProduct("all_pack", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }
    
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyProductID(string productId)
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            Debug.Log("Purchasing is disabled for RuStore build: " + productId);
            return;
#endif
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase: " + productId);

                    var all = m_StoreController.products.all;
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        
        
        public void RestorePurchases()
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            Debug.Log("RestorePurchases skipped: purchasing is disabled for RuStore build.");
            return;
#endif
            if (!IsInitialized())
            {
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }
            
            if (Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("RestorePurchases started ...");
                
//                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
//                apple.RestoreTransactions((result) => {
//                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
//                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }
        
        
        //  
        // --- IStoreListener
        //
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");
            
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            _storeController.UpdateSkins();
            _storeController.RefreshPrices();
        }
        
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"OnInitializeFailed InitializationFailureReason:{error}. {message}");
        }
        
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var iapTime = PlayerPrefs.GetFloat("IAPTime", 0);
            if (iapTime >= 0)
            {
                iapTime += Time.time;
                // _firebaseController.SimpleIntLog("FirstIAP", "Time", (int)iapTime);
                PlayerPrefs.SetFloat("IAPTime", -1);
            }
            if (string.Equals(args.purchasedProduct.definition.id, "skin10", StringComparison.Ordinal))
            {
                PlayerPrefs.SetInt("Skin9", 1);
                _storeController.UpdateSkins();
                _storeController.RefreshPrices();
            }

            if (string.Equals(args.purchasedProduct.definition.id, "skin11", StringComparison.Ordinal))
            {
                PlayerPrefs.SetInt("Skin10", 1);
                _storeController.UpdateSkins();
                _storeController.RefreshPrices();
            }

            if (string.Equals(args.purchasedProduct.definition.id, "skin12", StringComparison.Ordinal))
            {
                PlayerPrefs.SetInt("Skin11", 1);
                _storeController.UpdateSkins();
                _storeController.RefreshPrices();
            }
            
            if (string.Equals(args.purchasedProduct.definition.id, "no_ads", StringComparison.Ordinal))
            {
                _uiController.SetNoAds();
            }

            if (string.Equals(args.purchasedProduct.definition.id, "all_pack", StringComparison.Ordinal))
            {
                PlayerPrefs.SetInt("Skin9", 1);
                PlayerPrefs.SetInt("Skin10", 1);
                PlayerPrefs.SetInt("Skin11", 1);
                _uiController.SetNoAds();
                _storeController.UpdateSkins();
                _storeController.RefreshPrices();
            }


            return PurchaseProcessingResult.Complete;
        }
        
        
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // this reason with the user to guide their troubleshooting actions.
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public string GetLocalizedPrice(string iapId)
        {
#if UNITY_ANDROID && RUSTORE_BUILD
            return string.Empty;
#endif
            if (!IsInitialized())
                return string.Empty;

            var product = m_StoreController.products.WithID(iapId);
            if (product == null || product.metadata == null)
                return string.Empty;

            return string.IsNullOrEmpty(product.metadata.localizedPriceString)
                ? product.metadata.localizedPrice.ToString(CultureInfo.CurrentCulture)
                : product.metadata.localizedPriceString;
        }
}
#else
public class UnityStore
{
    [Inject] private StoreController _storeController;

    [Inject]
    private void Construct()
    {
        _storeController.UpdateSkins();
        _storeController.RefreshPrices();
    }

    public void InitializePurchasing()
    {
        Debug.Log("Purchasing is disabled for this build.");
    }

    public void BuyProductID(string productId)
    {
        Debug.Log("Purchasing is disabled for this build: " + productId);
    }

    public void RestorePurchases()
    {
        Debug.Log("RestorePurchases skipped: purchasing is disabled for this build.");
    }

    public string GetLocalizedPrice(string iapId)
    {
        return string.Empty;
    }
}
#endif
