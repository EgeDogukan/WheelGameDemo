using UnityEngine;
using WheelGame.Domain;

namespace WheelGame.Config
{
    [CreateAssetMenu(menuName = "Slice Config")]
    public class SliceConfig : ScriptableObject
    {
        [Header("Identity")]
        public string id;   
        
        [Header("Behaviour")]
        public SliceType sliceType;

        [Header("Reward Data (Ignored if bomb)")]
        public RewardType rewardType;
        
        [Header("Base amount to scale")]
        public int baseAmount;

        [Header("Sprite icon")]
        public Sprite icon;
    }
}