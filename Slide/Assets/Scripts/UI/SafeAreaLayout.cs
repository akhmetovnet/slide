using UnityEngine;

namespace UI
{
    /// <summary>
    /// Insets one stretch-to-parent UI region to the device safe area while
    /// leaving the background outside it. It is intentionally suitable for
    /// menu roots and scroll viewports, not for full-screen overlays.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _topReservedSpace;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void OnEnable()
        {
            _rectTransform = transform as RectTransform;
            ApplySafeArea();
        }

        private void Update()
        {
            if (_lastScreenSize.x != Screen.width ||
                _lastScreenSize.y != Screen.height ||
                _lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (_rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            var safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            _rectTransform.anchorMin = new Vector2(
                safeArea.xMin / Screen.width,
                safeArea.yMin / Screen.height);
            _rectTransform.anchorMax = new Vector2(
                safeArea.xMax / Screen.width,
                safeArea.yMax / Screen.height);
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = new Vector2(0f, -_topReservedSpace);
        }

        public void SetTopReservedSpace(float value)
        {
            _topReservedSpace = Mathf.Max(0f, value);
            ApplySafeArea();
        }
    }
}
