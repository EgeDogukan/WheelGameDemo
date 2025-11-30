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
             // Always at least baseAmount, even if growthPerZone = 0
            float growth = _config.growthPerZone; // e.g. 0.1 = +10% per zone
            float factor = 1f + growth * (zoneIndex - 1);

            float raw = baseAmount * factor;
            int result = Mathf.RoundToInt(raw);

            Debug.Log($"[Progression] base={baseAmount}, zone={zoneIndex}, " +
                      $"growth={growth}, factor={factor}, raw={raw}, result={result}");

            return result;
        }
    }
}