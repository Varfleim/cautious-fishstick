
using System;

using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public class DEcORAEOBuilding : IComparable, IEquatable<DEcORAEOBuilding>
    {
        public DEcORAEOBuilding(
            EcsPackedEntity buildingPE)
        {
            this.buildingPE = buildingPE;

            buildingPriority = 0;
        }

        public readonly EcsPackedEntity buildingPE;

        public int buildingPriority;


        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            DEcORAEOBuilding otherTarget = obj as DEcORAEOBuilding;

            if (otherTarget != null)
            {
                return buildingPriority.CompareTo(otherTarget.buildingPriority);
            }
            else
            {
                throw new ArgumentException("Object is not a DEcORAEOBuilding");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DEcORAEOBuilding objAsFleetRegion = obj as DEcORAEOBuilding;

            if (objAsFleetRegion == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsFleetRegion);
            }
        }
        public override int GetHashCode()
        {
            return buildingPE.GetHashCode();
        }
        public bool Equals(DEcORAEOBuilding other)
        {
            if (other == null)
            {
                return false;
            }

            return buildingPE.EqualsTo(other.buildingPE);
        }
    }
}