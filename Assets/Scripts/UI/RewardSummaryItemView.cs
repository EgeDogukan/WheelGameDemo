using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.UI
{
    public class RewardSummaryItemView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_reward_icon_value;
        [SerializeField] private TMP_Text ui_text_reward_amount_value;

        private void OnValidate()
        {
            if (ui_image_reward_icon_value == null)
                ui_image_reward_icon_value =
                    UIAutoBinder.FindComponentInChildren<Image>(transform, "ui_image_reward_icon_value");

            if (ui_text_reward_amount_value == null)
                ui_text_reward_amount_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_reward_amount_value");
        }

        public void Configure(Sprite icon, int amount)
        {
            if (ui_image_reward_icon_value != null)
                ui_image_reward_icon_value.sprite = icon;
                ui_image_reward_icon_value.preserveAspect = true;

            if (ui_text_reward_amount_value != null)
                ui_text_reward_amount_value.text = $"x{amount}";
        }
    }
}
