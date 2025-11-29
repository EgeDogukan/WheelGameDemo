// open closed principle
namespace WheelGame.Domain
{
    // decides if a zone is normal, silver or gold
    public interface IZoneTypeResolver
    {
        ZoneType GetZoneTypeForZoneIndex(int zoneIndex);
    }

    // provides the slices for a specific wheel
    public interface IWheelDefinitionProvider
    {
        WheelDefinition GetWheelFor(ZoneType zoneType, int zoneIndex);
    }

    // to calculate the math of reward scaling, since as player progresses "there is a chance to increase the reward that they will receive"
    public interface IRewardProgressionStrategy
    {
        int GetAmount(int baseAmount, int zoneIndex);
    }

    // wraps a random generation so to mock it in testing
    public interface IRandomProvider
    {
        int NextInt(int minInclusive, int maxExclusive);
    }
}