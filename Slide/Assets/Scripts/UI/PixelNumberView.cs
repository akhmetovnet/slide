using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class PixelNumberView : MonoBehaviour
    {
        [SerializeField] private Sprite[] _digits;
        [SerializeField] private Sprite _meterSuffix;
        [SerializeField] private Image[] _slots;

        public void SetValue(int value, bool showMeterSuffix)
        {
            var text = Mathf.Max(0, value).ToString();
            var requiredSlots = text.Length + (showMeterSuffix ? 1 : 0);

            for (var index = 0; index < _slots.Length; index++)
            {
                var slot = _slots[index];
                if (slot == null)
                    continue;

                var isVisible = index < requiredSlots;
                slot.gameObject.SetActive(isVisible);
                if (!isVisible)
                    continue;

                if (index < text.Length)
                {
                    var digit = text[index] - '0';
                    slot.sprite = digit >= 0 && digit < _digits.Length ? _digits[digit] : null;
                }
                else
                {
                    slot.sprite = _meterSuffix;
                }

                slot.preserveAspect = true;
                slot.raycastTarget = false;
            }
        }
    }
}
