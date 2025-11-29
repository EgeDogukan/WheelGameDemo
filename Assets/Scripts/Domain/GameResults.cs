namespace WheelGame.Domain
{

    // result of a single spin
    public sealed class SpinResult
    {
        public int ZoneIndex { get; }
        public ZoneType ZoneType {get;}
        public Slice LandedSlice { get; }
        public int RewardDelta { get; }
        public int TotalReward { get; }
        public bool HitBomb { get; }

        public SpinResult( int zoneIndex,
                           ZoneType zoneType,
                           Slice slice,
                           int rewardDelta,
                           int totalReward,
                           bool hitBomb)
        {
            ZoneIndex = zoneIndex;
            ZoneType = zoneType;
            LandedSlice = slice;
            RewardDelta = rewardDelta;
            TotalReward = totalReward;
            HitBomb = hitBomb;
        }
    }

    public sealed class LeaveResult
    {
        public int ZoneIndex {get;}
        public int TotalReward {get;}

        public LeaveResult(int zoneIndex, int totalReward)
        {
            ZoneIndex = zoneIndex;
            TotalReward = totalReward;
        }
    }
}