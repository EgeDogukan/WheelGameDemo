using System;
using System.Collections.Generic;
using UnityEngine;
using WheelGame.Domain;

namespace WheelGame.UI
{
    [Serializable]
    public class RewardVisualDefinition
    {
        public RewardType type;
        public Sprite icon;
    }

    public class RewardSummaryView : MonoBehaviour
    {
        [SerializeField] private RectTransform ui_reward_summary_container;
        [SerializeField] private RewardSummaryItemView itemPrefab;
        [SerializeField] private List<RewardVisualDefinition> rewardVisuals;

        private readonly List<RewardSummaryItemView> _items = new();

        private void OnValidate()
        {
            if (ui_reward_summary_container == null)
            {
                var t = UIAutoBinder.FindComponentInChildren<RectTransform>(
                    transform, "ui_reward_summary_container");
                if (t != null) ui_reward_summary_container = t;
            }

            if (itemPrefab == null)
            {
                itemPrefab = GetComponentInChildren<RewardSummaryItemView>(true);
            }
        }

        public void SetTotals(IReadOnlyDictionary<RewardType, int> totals)
        {
            ClearItems();

            if (totals == null || ui_reward_summary_container == null || itemPrefab == null)
                return;

            foreach (var kvp in totals)
            {
                var type = kvp.Key;
                var amount = kvp.Value;
                if (amount <= 0) continue;

                var icon = GetIconFor(type);

                var item = Instantiate(itemPrefab, ui_reward_summary_container);
                item.gameObject.name = $"ui_reward_summary_item_{type}";
                item.gameObject.SetActive(true);
                item.Configure(icon, amount);
                _items.Add(item);
            }
        }

        private Sprite GetIconFor(RewardType type)
        {
            foreach (var def in rewardVisuals)
            {
                if (def.type == type)
                    return def.icon;
            }
            return null;
        }

        private void ClearItems()
        {
            foreach (var item in _items)
            {
                if (item == null) continue;
                if (Application.isPlaying) Destroy(item.gameObject);
                else DestroyImmediate(item.gameObject);
            }
            _items.Clear();
        }
    }
}
