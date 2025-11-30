using System.Collections.Generic;
using UnityEngine;
using WheelGame.Domain;

namespace WheelGame.Config
{
    [CreateAssetMenu(menuName = "Wheel Layout Config")]
    public class WheelLayoutConfig : ScriptableObject
    {
        [Header("Zone Type")]
        public ZoneType zoneType;

        [Header("Slices")]
        public List<SliceConfig> slices;
    }
}