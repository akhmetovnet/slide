using System;
using UnityEngine;

namespace UI
{
    public enum RestartOfferType
    {
        Skin,
        Bundle
    }

    public enum RestartOfferFallbackState
    {
        Disabled,
        Hidden
    }

    [Serializable]
    public sealed class RestartOfferDefinition
    {
        [SerializeField] private string _productId;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        [SerializeField] private int _offerOrder;
        [SerializeField] private RestartOfferType _offerType;
        [SerializeField] private RestartOfferFallbackState _fallbackState;

        public string ProductId => _productId;
        public Sprite Icon => _icon;
        public string Title => _title;
        public string Description => _description;
        public int OfferOrder => _offerOrder;
        public RestartOfferType OfferType => _offerType;
        public RestartOfferFallbackState FallbackState => _fallbackState;

        public RestartOfferDefinition(
            string productId,
            Sprite icon,
            string title,
            string description,
            int offerOrder,
            RestartOfferType offerType,
            RestartOfferFallbackState fallbackState)
        {
            _productId = productId;
            _icon = icon;
            _title = title;
            _description = description;
            _offerOrder = offerOrder;
            _offerType = offerType;
            _fallbackState = fallbackState;
        }
    }
}
