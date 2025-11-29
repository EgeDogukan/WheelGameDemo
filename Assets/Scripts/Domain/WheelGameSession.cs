using System;
namespace WheelGame.Domain
{
    // Since this whole code is pure logic written in c# and has no relationship to unity, 
    // we could port this to a server side back end to prevent cheating so that spins are calculated server side.
    // Also this is fully testable independent of unity, no statics etc. 
    public sealed class WheelGameSession
    {
        private readonly IZoneTypeResolver _zoneTypeResolver;
        private readonly IWheelDefinitionProvider _wheelProvider;
        private readonly IRewardProgressionStrategy _progression;
        private readonly IRandomProvider _random;

        public int CurrentZoneIndex { get; private set; } = 1;
        public int TotalReward { get; private set;}
        public bool IsBombHit { get; private set;}
        public bool IsFinished { get; private set; }

        public ZoneType CurrentZoneType => _zoneTypeResolver.GetZoneTypeForZoneIndex(CurrentZoneIndex);

        public WheelGameSession(IZoneTypeResolver zoneTypeResolver, 
                                IWheelDefinitionProvider wheelProvider,
                                IRewardProgressionStrategy progression,
                                IRandomProvider random)
        {
            _zoneTypeResolver = zoneTypeResolver;
            _wheelProvider = wheelProvider;
            _progression = progression;
            _random = random;
        }

        // checking if can leave or an spin based on requirements
        public bool CanSpin => !IsFinished && !IsBombHit;
        public bool CanLeave => !IsFinished && !IsBombHit && (CurrentZoneType == ZoneType.SafeSilver ||
                                                              CurrentZoneType == ZoneType.SuperGold);

        // step 1, choose which slice wins, for ui spin animation
        public int ChooseSliceIndex()
        {
            var wheel = _wheelProvider.GetWheelFor(CurrentZoneType, CurrentZoneIndex);
            return _random.NextInt(0, wheel.Slices.Count);
        }

        // step 2, after animation, commit the result
        public SpinResult ResolveSpin(int sliceIndex)
        {
            if(!CanSpin)
            {
                throw new InvalidOperationException("Cannot spin in current state.");
            }

            var wheel = _wheelProvider.GetWheelFor(CurrentZoneType, CurrentZoneIndex);
            if (sliceIndex < 0 || sliceIndex >= wheel.Slices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex));
            }        
            var slice = wheel.Slices[sliceIndex];

            int delta = 0;
            bool bomb = slice.IsBomb;

            if(bomb) // lost everything case
            {
                TotalReward = 0;
                IsBombHit = true;
                IsFinished = true;
            }
            else if (slice.Reward != null)
            {
                var baseAmount = slice.Reward.Amount;   // calculate reward based on zone depth so we dont have to set up each turn by hand
                var scaledAmount = _progression.GetAmount(baseAmount, CurrentZoneIndex);    
            }

            var result = new SpinResult(CurrentZoneIndex,
                                        wheel.ZoneType,
                                        slice,
                                        delta,
                                        TotalReward,
                                        bomb);
            
            if (!bomb)
            {
                CurrentZoneIndex++;
            }

            return result;
        }

        public LeaveResult Leave()
        {
            if(!CanLeave)
            {
                throw new InvalidOperationException("Cannot leave from this zone.");
            }
            IsFinished = true;
            return new LeaveResult(CurrentZoneIndex, TotalReward);
        }

        // future revive mechanic
        /*
        public void Revive()
        {
            if (!IsBombHit) return;
            IsBombHit = false;
            IsFinished = false;
        }
        */
    }
}