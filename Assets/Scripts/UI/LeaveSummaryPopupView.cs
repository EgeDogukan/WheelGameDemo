using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Domain;

namespace WheelGame.UI
{
    public class LeaveSummaryPopupView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupContent;

        [SerializeField] private TMP_Text ui_text_leave_title_value;
        [SerializeField] private TMP_Text ui_text_leave_total_value;

        [SerializeField] private RewardSummaryView rewardSummaryView;
        [SerializeField] private Button ui_button_leave_play_again;

        [Header("Animation")]
        [SerializeField] private float fadeDuration  = 0.25f;
        [SerializeField] private float scaleDuration = 0.25f;
        [SerializeField] private float hiddenScale   = 0.7f;
        [SerializeField] private float shownScale    = 1.0f;

        private Action _onPlayAgain;
        private Tween _currentTween;

        private void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (popupContent == null)
            {
                var t = UIAutoBinder.FindComponentInChildren<RectTransform>(
                    transform, "ui_panel_leave_popup");
                if (t != null) popupContent = t;
            }

            if (ui_text_leave_title_value == null)
                ui_text_leave_title_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_leave_title_value");

            if (ui_text_leave_total_value == null)
                ui_text_leave_total_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_leave_total_value");

            if (rewardSummaryView == null)
                rewardSummaryView = GetComponentInChildren<RewardSummaryView>(true);

            if (ui_button_leave_play_again == null)
                ui_button_leave_play_again =
                    UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_leave_play_again");
        }

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Ensure we start invisible, but DO NOT disable the gameObject here
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false; // block clicks when hidden
            }

            if (popupContent != null)
                popupContent.localScale = Vector3.one * hiddenScale;
            
            // REMOVED: gameObject.SetActive(false); 
            // If you want it hidden at start, uncheck the box in the Inspector,
            // or use Start() to disable it, but Awake is unsafe for this logic.
        }

        public void SetPlayAgainCallback(Action onPlayAgain)
        {
            _onPlayAgain = onPlayAgain;

            if (ui_button_leave_play_again != null)
            {
                ui_button_leave_play_again.onClick.RemoveAllListeners();
                ui_button_leave_play_again.onClick.AddListener(OnPlayAgainClickedInternal);
            }
        }

        public void Show(
            IReadOnlyDictionary<RewardType, int> totals,
            int totalReward)
        {
            // 1. Ensure object is active so Coroutines/Tweens run
            gameObject.SetActive(true);

            _currentTween?.Kill();

            // 2. Reset visual state for entry animation
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true; // Enable interactions
            }
            
            if (popupContent != null)
                popupContent.localScale = Vector3.one * hiddenScale;

            // 3. Fill Data
            if (ui_text_leave_title_value != null)
                ui_text_leave_title_value.text = "Rewards";

            if (ui_text_leave_total_value != null)
                ui_text_leave_total_value.text = $"Total: {totalReward}";

            rewardSummaryView?.SetTotals(totals);

            // 4. Animate
            var seq = DOTween.Sequence();

            if (canvasGroup != null)
                seq.Join(canvasGroup.DOFade(1f, fadeDuration));

            if (popupContent != null)
                seq.Join(popupContent.DOScale(shownScale, scaleDuration).SetEase(Ease.OutBack));

            _currentTween = seq;
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _currentTween?.Kill();
            
            // Disable interactions immediately so user can't double click while fading
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false; 

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

        private void OnPlayAgainClickedInternal()
        {
            _onPlayAgain?.Invoke();
        }
    }
}