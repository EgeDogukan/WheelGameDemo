using WheelGame.Domain;

namespace WheelGame.Adapters
{
    public class ScriptableZoneTypeResolver : IZoneTypeResolver
    {
        private const int SuperZoneInterval = 30;
        private const int SafeZoneInterval = 5;
        public ZoneType GetZoneTypeForZoneIndex(int zoneIndex)
        {
            if(zoneIndex % SuperZoneInterval == 0)
            {
                return ZoneType.SuperGold;
            }
            if(zoneIndex % SafeZoneInterval == 0)
            {
                return ZoneType.SafeSilver;
            }
            return ZoneType.Normal;
        }
    }
}