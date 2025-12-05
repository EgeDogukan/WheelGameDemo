using TMPro;
using UnityEngine;
using WheelGame.Domain;

namespace WheelGame.UI
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text ui_text_zone_index_value;
        [SerializeField] private TMP_Text ui_text_zone_type_value;
        [SerializeField] private TMP_Text ui_text_total_reward_value;
        [SerializeField] private TMP_Text ui_text_next_safe_zone_value;
        [SerializeField] private TMP_Text ui_text_next_super_zone_value;

        private void OnValidate()
        {
            if (ui_text_zone_index_value == null)
                ui_text_zone_index_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_zone_index_value");

            if (ui_text_zone_type_value == null)
                ui_text_zone_type_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_zone_type_value");

            if (ui_text_total_reward_value == null)
                ui_text_total_reward_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_total_reward_value");

            if (ui_text_next_safe_zone_value == null)
                ui_text_next_safe_zone_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_next_safe_zone_value");

            if (ui_text_next_super_zone_value == null)
                ui_text_next_super_zone_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_next_super_zone_value");
        }

        public void SetZone(int zoneIndex, ZoneType zoneType)
        {
            ui_text_zone_index_value.text = zoneIndex.ToString();
            ui_text_zone_type_value.text  = GetZoneDisplayName(zoneType);
        }

        private static string GetZoneDisplayName(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Normal     => "Normal Zone",
                ZoneType.SafeSilver => "Safe Silver Zone",
                ZoneType.SuperGold  => "Super Gold Zone",
                _                   => zoneType.ToString()
            };
        }

        public void SetTotalReward(int total)
        {
            if (ui_text_total_reward_value != null)
                ui_text_total_reward_value.text = total.ToString();
        }

        public void SetNextSafeZone(int zoneIndex)
        {
            if (ui_text_next_safe_zone_value != null)
                ui_text_next_safe_zone_value.text = zoneIndex.ToString();
        }

        public void SetNextSuperZone(int zoneIndex)
        {
            if (ui_text_next_super_zone_value != null)
                ui_text_next_super_zone_value.text = zoneIndex.ToString();
        }
    }
}
