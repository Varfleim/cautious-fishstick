
using System;

namespace SandOcean.Warfare.Fleet
{
    public class DFleetRegionPriority : IComparable
    {
        public DFleetRegionPriority(
            int regionIndex, float regionPriority)
        {
            this.regionIndex = regionIndex;
            this.regionPriority = regionPriority;
        }

        public readonly int regionIndex;
        public readonly float regionPriority;

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            DFleetRegionPriority otherRegion = obj as DFleetRegionPriority;

            if (otherRegion != null)
            {
                return regionPriority.CompareTo(otherRegion.regionPriority);
            }
            else
            {
                throw new ArgumentException("Object is not a DFleetRegionPriority");
            }
        }
    }
}