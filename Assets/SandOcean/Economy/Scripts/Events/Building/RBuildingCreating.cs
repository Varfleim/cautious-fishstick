
using Leopotam.EcsLite;

namespace SandOcean.Economy.Building
{
    public readonly struct RBuildingCreating
    {
        public RBuildingCreating(
            EcsPackedEntity parentORAEOPE,
            DBuildingType buildingType)
        {
            this.parentORAEOPE = parentORAEOPE;

            this.buildingType = buildingType;
        }

        public readonly EcsPackedEntity parentORAEOPE;

        public readonly DBuildingType buildingType;
    }
}