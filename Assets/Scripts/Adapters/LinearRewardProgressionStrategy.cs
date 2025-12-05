using UnityEngine;
using WheelGame.Config;
using WheelGame.Domain;

namespace WheelGame.Adapters
{
    public class LinearRewardProgressionStrategy : IRewardProgressionStrategy
    {
        private readonly RewardProgressionConfig _config;
        
        public LinearRewardProgressionStrategy(RewardProgressionConfig config)
        {
            _config = config;
        }

        public int GetAmount(int baseAmount, int zoneIndex)
        {
            float growth = _config.growthPerZone; // e.g. 0.1 = +10% per zone
            float factor = 1f + growth * (zoneIndex - 1);

            float raw = baseAmount * factor;
            int result = Mathf.RoundToInt(raw);

            return result;
        }
    }
}