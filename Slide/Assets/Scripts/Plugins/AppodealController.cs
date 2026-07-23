// using AppodealAds.Unity.Api;
// using AppodealAds.Unity.Common;
// using Signals;
// using UI;
// using UnityEngine;
// using Zenject;

// namespace Plugins
// {
//     public class AppodealController  : IRewardedVideoAdListener, IBannerAdListener, IInterstitialAdListener
//     {
//         [Inject] private UIController _uiController;
//         
//         private SignalBus _signalBus;
//         private bool _isContinue;
//         
//         public AppodealController(SignalBus signalBus)
//         {
//             _signalBus = signalBus;
// #if UNITY_ANDROID
//             Appodeal.initialize("9ad9d97de2e3ef65fcf93374eda7f134a57cceb7da72ec53", Appodeal.BANNER | Appodeal.REWARDED_VIDEO | Appodeal.INTERSTITIAL, false);
// #else
//             Appodeal.initialize("593d94cd6ce839d832f69a10de192ed3365c3e6297b3972c", Appodeal.BANNER | Appodeal.REWARDED_VIDEO | Appodeal.INTERSTITIAL, false);
// #endif
//             
//             Appodeal.setRewardedVideoCallbacks(this);
//             Appodeal.setBannerCallbacks(this);
//             Appodeal.setInterstitialCallbacks(this);
//         }
//         
//         #region RewardedVideo
//
//         public bool RewardedVideoIsLoaded => Appodeal.isLoaded(Appodeal.REWARDED_VIDEO);
//
//         public void ShowRewardedVideo(bool isContinue)
//         {
//             _isContinue = isContinue;
//             Appodeal.show(Appodeal.REWARDED_VIDEO);
//             if (_uiController.SoundIsOn)
//                 _uiController.Mixer.SetFloat("Master", -80);
//         }
//         public void onRewardedVideoLoaded(bool isPrecache) { }
//         public void onRewardedVideoFailedToLoad() {  }
//         public void onRewardedVideoShowFailed() { }
//
//         public void onRewardedVideoShown() {  }
//
//         public void onRewardedVideoClosed(bool finished)
//         {
//             _signalBus.Fire(new Video() {isContinue = _isContinue, isFinished = finished});
//             if (_uiController.SoundIsOn)
//                 _uiController.Mixer.SetFloat("Master", 0);
//         }
//
//         public void onRewardedVideoFinished(double amount, string name) {  }
//         public void onRewardedVideoExpired() {  }
//         public void onRewardedVideoClicked()
//         { }
//
//         #endregion
//
//         #region Banner
//
//         public void ShowBanner(bool isShow)
//         {
//             if(isShow)
//                 Appodeal.show(Appodeal.BANNER_BOTTOM);
//             else
//                 Appodeal.hide(Appodeal.BANNER_BOTTOM);
//         }
//         public void onBannerLoaded(int height, bool isPrecache) {  }
//         public void onBannerFailedToLoad() {  }
//         public void onBannerShown() {  }
//         public void onBannerClicked() {  }
//         public void onBannerExpired() {  }
//         #endregion
//         
//         #region Interstitial callback handlers
//         
//         public bool InterstitialVideoIsLoaded => Appodeal.isLoaded(Appodeal.REWARDED_VIDEO);
//
//         public void SHowInterstital()
//         {
//             Appodeal.show(Appodeal.INTERSTITIAL);
//         }
//         public void onInterstitialLoaded(bool isPrecache) { Debug.Log("Interstitial loaded"); }
//         public void onInterstitialFailedToLoad() { Debug.Log("Interstitial failed"); }
//         public void onInterstitialShowFailed() { }
//         public void onInterstitialShown() { Debug.Log("Interstitial opened"); }
//         public void onInterstitialClosed() { Debug.Log("Interstitial closed"); }
//         public void onInterstitialClicked() { Debug.Log("Interstitial clicked"); }
//         public void onInterstitialExpired() { Debug.Log("Interstitial expired"); }
//         #endregion
//     }
// }
