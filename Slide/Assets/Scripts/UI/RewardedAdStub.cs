using System;
using System.Collections;
using Installers;
using UnityEngine;
using Zenject;

namespace UI
{
    /// <summary>
    /// Temporary rewarded-ad adapter. It deliberately keeps the UI unaware of
    /// the implementation so an SDK-backed adapter can replace it later.
    /// </summary>
    public sealed class RewardedAdStub : MonoBehaviour, IRewardedAdService
    {
        [Inject] private SoInstaller.GameSettings _settings;

        private Coroutine _request;
        private Action _successCallback;

        public bool IsShowing => _request != null;

        public void Show(Action successCallback)
        {
            if (_request != null)
                return;

            _successCallback = successCallback;
            _request = StartCoroutine(CompleteAfterDelay());
            Debug.Log("RewardedAdStub: test rewarded ad started.");
        }

        public void Cancel()
        {
            if (_request != null)
                StopCoroutine(_request);

            _request = null;
            _successCallback = null;
        }

        private IEnumerator CompleteAfterDelay()
        {
            var delay = Mathf.Max(0f, _settings.rewardedAdStubDelay);
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            var callback = _successCallback;
            _successCallback = null;
            _request = null;
            Debug.Log("RewardedAdStub: test rewarded ad completed.");
            callback?.Invoke();
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnDestroy()
        {
            Cancel();
        }
    }
}
