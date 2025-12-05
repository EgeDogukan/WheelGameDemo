using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        // We keep track of items to reuse them instead of destroying them
        private readonly Dictionary<RewardType, RewardSummaryItemView> _activeItems = new();

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
            if (totals == null || ui_reward_summary_container == null || itemPrefab == null)
                return;
            if (totals == null || totals.Count == 0)
            {
                ClearAll();
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui_reward_summary_container);
                return;
            }

            bool layoutChanged = false;

            // update or creat items in the list to fly to
            foreach (var kvp in totals)
            {
                var type = kvp.Key;
                var amount = kvp.Value;
                if (amount <= 0) continue;

                // check if we already have a row for this reward type and create or update
                if (_activeItems.TryGetValue(type, out var existingItem))
                {
                    var icon = GetIconFor(type);
                    existingItem.Configure(icon, amount, type);
                }
                else
                {
                    var icon = GetIconFor(type);
                    var newItem = Instantiate(itemPrefab, ui_reward_summary_container);
                    newItem.gameObject.name = $"ui_reward_summary_item_{type}";
                    newItem.gameObject.SetActive(true);
                    newItem.Configure(icon, amount, type);

                    _activeItems[type] = newItem;
                    layoutChanged = true;
                }
            }
            

            if (layoutChanged)
            {
                // rebuild the container so items arrange themselves so the animation target destination is correct before the fly starts.
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui_reward_summary_container);
                
                if (ui_reward_summary_container.parent != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_reward_summary_container.parent as RectTransform);
                }
            }
        }

        public RectTransform GetAnchorFor(RewardType type)
        {
            if (_activeItems.TryGetValue(type, out var item) && item != null)
                return item.IconRect;

            return null;
        }

        public void ClearAll()
        {
            foreach (var item in _activeItems.Values)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _activeItems.Clear();
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
    }
}