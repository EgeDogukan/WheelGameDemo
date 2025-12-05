using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.UI
{
    public class BombPopupView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupContent;

        [SerializeField] private Button ui_button_bomb_give_up;
        [SerializeField] private Button ui_button_bomb_revive_currency;
        [SerializeField] private Button ui_button_bomb_revive_ad;

        [Header("Animation")]
        [SerializeField] private float fadeDuration  = 0.25f;
        [SerializeField] private float scaleDuration = 0.25f;
        [SerializeField] private float hiddenScale   = 0.7f;
        [SerializeField] private float shownScale    = 1.0f;

        private Action _onGiveUp;
        private Action _onReviveCurrency;
        private Action _onReviveAd;
        private Tween _currentTween;

        private void OnValidate()
        {
            if (canvasGroup == null)
        canvasGroup = GetComponent<CanvasGroup>();

            if (popupContent == null)
            {
                var t = UIAutoBinder.FindComponentInChildren<RectTransform>(
                    transform, "ui_panel_bomb_popup");
                if (t != null) popupContent = t;
            }

            if (ui_button_bomb_give_up == null)
                ui_button_bomb_give_up = UIAutoBinder.FindComponentInChildren<Button>(
                    transform, "ui_button_give_up"); 

            if (ui_button_bomb_revive_currency == null)
                ui_button_bomb_revive_currency = UIAutoBinder.FindComponentInChildren<Button>(
                    transform, "ui_button_revive_currency"); 

            if (ui_button_bomb_revive_ad == null)
                ui_button_bomb_revive_ad = UIAutoBinder.FindComponentInChildren<Button>(
                    transform, "ui_button_revive_ad"); 
        }

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // initial hidden state
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (popupContent != null)
                popupContent.localScale = Vector3.one * hiddenScale;

            gameObject.SetActive(false);
        }

        public void SetCallbacks(Action onGiveUp, Action onReviveCurrency, Action onReviveAd)
        {
            _onGiveUp         = onGiveUp;
            _onReviveCurrency = onReviveCurrency;
            _onReviveAd       = onReviveAd;

            ClearButtonListeners();

            if (ui_button_bomb_give_up != null)
                ui_button_bomb_give_up.onClick.AddListener(OnGiveUpClickedInternal);

            if (ui_button_bomb_revive_currency != null && _onReviveCurrency != null)
                ui_button_bomb_revive_currency.onClick.AddListener(OnReviveCurrencyClickedInternal);

            if (ui_button_bomb_revive_ad != null && _onReviveAd != null)
                ui_button_bomb_revive_ad.onClick.AddListener(OnReviveAdClickedInternal);
        }

        public void ClearCallbacks()
        {
            _onGiveUp         = null;
            _onReviveCurrency = null;
            _onReviveAd       = null;

            ClearButtonListeners();
        }

        private void ClearButtonListeners()
        {
            if (ui_button_bomb_give_up != null)
                ui_button_bomb_give_up.onClick.RemoveAllListeners();

            if (ui_button_bomb_revive_currency != null)
                ui_button_bomb_revive_currency.onClick.RemoveAllListeners();

            if (ui_button_bomb_revive_ad != null)
                ui_button_bomb_revive_ad.onClick.RemoveAllListeners();
        }

        // Called from controller when bomb is hit
        public void Show()
        {
            gameObject.SetActive(true);

            _currentTween?.Kill();

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (popupContent != null)
                popupContent.localScale = Vector3.one * hiddenScale;

            var seq = DOTween.Sequence();

            if (canvasGroup != null)
                seq.Join(canvasGroup.DOFade(1f, fadeDuration));

            if (popupContent != null)
                seq.Join(popupContent.DOScale(shownScale, scaleDuration).SetEase(Ease.OutBack));

            _currentTween = seq;
        }

        // Called from controller when you restart / close
        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _currentTween?.Kill();

            var seq = DOTween.Sequence();

            if (canvasGroup != null)
                seq.Join(canvasGroup.DOFade(0f, fadeDuration));

            if (popupContent != null)
                seq.Join(popupContent
                    .DOScale(hiddenScale, scaleDuration)
                    .SetEase(Ease.InBack));

            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

            _currentTween = seq;
        }

        private void OnGiveUpClickedInternal()
        {
            // let controller handle restart etc.
            _onGiveUp?.Invoke();
        }

        private void OnReviveCurrencyClickedInternal()
        {
            _onReviveCurrency?.Invoke();
        }

        private void OnReviveAdClickedInternal()
        {
            _onReviveAd?.Invoke();
        }
    }
}
