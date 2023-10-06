
using Leopotam.EcsLite;

namespace SandOcean.Warfare.Fleet.Moving
{
    public struct CTaskForceMovementToMoving
    {
        public CTaskForceMovementToMoving(
            EcsPackedEntity movingTargetLastRegionPE)
        {
            this.movingTargetLastRegionPE = movingTargetLastRegionPE;
        }

        public EcsPackedEntity movingTargetLastRegionPE;
    }
}