using System;
using System.Collections.Generic;
using System.Linq;
using WheelGame.Config;
using WheelGame.Domain;


namespace WheelGame.Adapters
{
    public class ScriptableWheelDefinitionProvider : IWheelDefinitionProvider
    {
        private readonly IReadOnlyDictionary<ZoneType, WheelLayoutConfig> _layouts;

        public ScriptableWheelDefinitionProvider(IEnumerable<WheelLayoutConfig> layouts)
        {
            _layouts = layouts.ToDictionary(layout => layout.zoneType);
        }
        
        public WheelDefinition GetWheelFor(ZoneType zoneType, int zoneIndex)
        {
            WheelLayoutConfig layoutConfig = GetLayoutConfigFor(zoneType);

            var domainSlices = new List<Slice>();

            foreach(var sliceConfig in layoutConfig.slices)
            {
                if(sliceConfig.sliceType == SliceType.Bomb)
                {
                    domainSlices.Add(new Slice(SliceType.Bomb, null));
                }
                else
                {
                    var reward = new Reward(sliceConfig.rewardType, sliceConfig.baseAmount);
                    domainSlices.Add(new Slice(SliceType.Reward, reward));
                }
            }
            return new WheelDefinition(layoutConfig.zoneType, domainSlices);
        }

        public WheelLayoutConfig GetLayoutConfigFor(ZoneType zoneType)
        {
            if(_layouts.TryGetValue(zoneType, out var layout))
            {
                return layout;
            }
            throw new ArgumentException($"No layout config for zone '{zoneType}'", nameof(zoneType));
        }
    }
}