
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.Warfare.Fleet.Moving
{
    public struct CTaskForceMovement
    {
        public CTaskForceMovement(int a)
        {
            pathRegionPEs = new(16);

            traveledDistance = 0;
            isTraveled = false;
        }

        public List<EcsPackedEntity> pathRegionPEs;

        public float traveledDistance;
        public bool isTraveled;
    }
}