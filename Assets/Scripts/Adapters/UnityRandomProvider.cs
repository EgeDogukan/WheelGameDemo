using UnityEngine;
using WheelGame.Domain;

namespace WheelGame.Adapters
{
    public class UnityRandomProvider : IRandomProvider
    {
        public int NextInt(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}