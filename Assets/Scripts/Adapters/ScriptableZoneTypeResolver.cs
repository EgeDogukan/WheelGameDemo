using WheelGame.Domain;

namespace WheelGame.Adapters
{
    public class ScriptableZoneTypeResolver : IZoneTypeResolver
    {
        private const int superZoneInterval = 30;
        private const int safeZoneInterval = 5;
        public ZoneType GetZoneTypeForZoneIndex(int zoneIndex)
        {
            if(zoneIndex % superZoneInterval == 0)
            {
                return ZoneType.SuperGold;
            }
            if(zoneIndex % safeZoneInterval == 0)
            {
                return ZoneType.SafeSilver;
            }
            return ZoneType.Normal;
        }
    }
}