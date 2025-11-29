#nullable enable
using System.Collections.Generic;

namespace WheelGame.Domain
{
    // enums
    public enum ZoneType
    {
        Normal,     // Normal one, contains bomb
        SafeSilver, // no bomb, standart rewrds every 5th
        SuperGold   // no bomb, big rewards every 30th
    }

    public enum SliceType
    {
        Reward,
        Bomb
    }

    public enum RewardType
    {
        Cash,
        Gold,
        Ticket
        // TODO: extend it later based on the rewards, need to think first if im gonna count knife and a gun both weapon, we will see
    }

    // core classes
    // we build slices from rewards, then build the wheel from both zonestates and slices
    public sealed class ZoneState
    {
        public int Index { get; }
        public ZoneType ZoneType { get; }

        public ZoneState(int index, ZoneType zoneType)
        {
            Index = index;
            ZoneType = zoneType;
        }
    }

    public sealed class Reward
    {
        public RewardType Type { get; }
        public int Amount { get; }

        public Reward(RewardType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public sealed class Slice
    {
        public SliceType SliceType { get;}
        public Reward? Reward { get; }
    
        
        public bool IsBomb => SliceType == SliceType.Bomb;

        public Slice(SliceType sliceType, Reward? reward)
        {
            SliceType = sliceType;
            Reward = reward;
            
        }
    }

    public sealed class WheelDefinition
    {
        public ZoneType ZoneType { get; }
        public IReadOnlyList<Slice> Slices { get; }

        public WheelDefinition(ZoneType zoneType, IReadOnlyList<Slice> slices)
        {
            ZoneType = zoneType;
            Slices = slices;
        }
    }
}