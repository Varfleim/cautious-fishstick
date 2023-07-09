
using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public struct CExplorationORAEO
    {
        public CExplorationORAEO(
            EcsPackedEntity selfPE,
            EcsPackedEntity organizationPE,
            EcsPackedEntity parentLAEOPE,
            byte explorationLevel)
        {
            this.selfPE = selfPE;

            this.organizationPE = organizationPE;

            this.parentRAEOPE = parentLAEOPE;

            this.explorationLevel = explorationLevel;
        }

        public readonly EcsPackedEntity selfPE;

        public readonly EcsPackedEntity organizationPE;

        public readonly EcsPackedEntity parentRAEOPE;

        public byte explorationLevel;
    }
}