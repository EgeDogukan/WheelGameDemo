using System;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.UI
{
    public class BombPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button ui_button_give_up;
        [SerializeField] private Button ui_button_revive_currency;
        [SerializeField] private Button ui_button_revive_ad;

        private Action _onGiveUp;
        private Action _onReviveCurrency;
        private Action _onReviveAd;

        private void OnValidate()
        {
            if (root == null)
                root = gameObject;

            if (ui_button_give_up == null)
                ui_button_give_up =
                    UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_give_up");

            if (ui_button_revive_currency == null)
                ui_button_revive_currency =
                    UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_revive_currency");

            if (ui_button_revive_ad == null)
                ui_button_revive_ad =
                    UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_revive_ad");
        }

        private void OnEnable()
        {
            if (ui_button_give_up != null)
                ui_button_give_up.onClick.AddListener(HandleGiveUpClicked);

            if (ui_button_revive_currency != null)
                ui_button_revive_currency.onClick.AddListener(HandleReviveCurrencyClicked);

            if (ui_button_revive_ad != null)
                ui_button_revive_ad.onClick.AddListener(HandleReviveAdClicked);
        }

        private void OnDisable()
        {
            if (ui_button_give_up != null)
                ui_button_give_up.onClick.RemoveListener(HandleGiveUpClicked);

            if (ui_button_revive_currency != null)
                ui_button_revive_currency.onClick.RemoveListener(HandleReviveCurrencyClicked);

            if (ui_button_revive_ad != null)
                ui_button_revive_ad.onClick.RemoveListener(HandleReviveAdClicked);
        }

        public void SetCallbacks(Action onGiveUp, Action onReviveCurrency, Action onReviveAd)
        {
            _onGiveUp = onGiveUp;
            _onReviveCurrency = onReviveCurrency;
            _onReviveAd = onReviveAd;
        }

        public void ClearCallbacks()
        {
            _onGiveUp = null;
            _onReviveCurrency = null;
            _onReviveAd = null;
        }

        public void Show()
        {
            if (root != null)
                root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void HandleGiveUpClicked()      => _onGiveUp?.Invoke();
        private void HandleReviveCurrencyClicked() => _onReviveCurrency?.Invoke();
        private void HandleReviveAdClicked()       => _onReviveAd?.Invoke();
    }
}
