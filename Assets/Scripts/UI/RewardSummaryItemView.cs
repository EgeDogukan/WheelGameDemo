using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Domain;

namespace WheelGame.UI
{
    public class RewardSummaryItemView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_reward_icon_value;
        [SerializeField] private TMP_Text ui_text_reward_amount_value;
        [SerializeField] private RewardType rewardType;

        private void OnValidate()
        {
            if (ui_image_reward_icon_value == null)
                ui_image_reward_icon_value =
                    UIAutoBinder.FindComponentInChildren<Image>(transform, "ui_image_reward_icon_value");

            if (ui_text_reward_amount_value == null)
                ui_text_reward_amount_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_reward_amount_value");
        }

        public RewardType RewardType => rewardType;

        // anchor we're gonna fly to
        public RectTransform IconRect =>
            ui_image_reward_icon_value != null
                ? (RectTransform)ui_image_reward_icon_value.transform
                : (RectTransform)transform;

        public void Configure(Sprite icon, int amount, RewardType type)
        {
            rewardType = type;

            if (ui_image_reward_icon_value != null)
            {
                ui_image_reward_icon_value.sprite = icon;
                ui_image_reward_icon_value.preserveAspect = true;
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = $"x{amount}";
            }
        }
    }
}
