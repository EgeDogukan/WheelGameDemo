using System.Collections.Generic;
using WheelGame.Config;
using WheelGame.Domain;


namespace WheelGame.Adapters
{
    public class ScriptableWheelDefinitionProvider : IWheelDefinitionProvider
    {
        private readonly WheelLayoutConfig _normal;
        private readonly WheelLayoutConfig _safe;
        private readonly WheelLayoutConfig _super;

        public ScriptableWheelDefinitionProvider(WheelLayoutConfig normal,
                                                 WheelLayoutConfig safe,
                                                 WheelLayoutConfig super
                                                 )
        {
            _normal = normal;
            _safe = safe;
            _super = super;
        }
        
        public WheelDefinition GetWheelFor(ZoneType zoneType, int zoneIndex)
        {
            WheelLayoutConfig layoutConfig = zoneType switch
            {
                ZoneType.Normal => _normal,
                ZoneType.SafeSilver => _safe,
                ZoneType.SuperGold => _super,
                _ => _normal
            };

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
    }
}