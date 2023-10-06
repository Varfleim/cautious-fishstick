
using Leopotam.EcsLite;

namespace SandOcean.Warfare.Fleet.Moving
{
    public readonly struct SRTaskForceFindPath
    {
        public SRTaskForceFindPath(
            EcsPackedEntity targetRegionPE)
        {
            this.targetRegionPE = targetRegionPE;
        }

        public readonly EcsPackedEntity targetRegionPE;
    }
}