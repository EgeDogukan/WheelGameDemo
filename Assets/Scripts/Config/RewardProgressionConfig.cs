using UnityEngine;

namespace WheelGame.Config
{
    [CreateAssetMenu(menuName = "Reward Progression Config")]
    public class RewardProgressionConfig : ScriptableObject
    {
        [Tooltip("Multiplier per zone, 0.1 = 10% increase in rewards.")]
        public float growthPerZone = 0.1f;
    }
}