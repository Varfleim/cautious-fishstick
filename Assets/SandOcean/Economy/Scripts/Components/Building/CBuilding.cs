
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.Economy.Building
{
    public struct CBuilding
    {
        public CBuilding(
            EcsPackedEntity selfPE, DBuildingType buildingType,
            EcsPackedEntity parentORAEOPE)
        {
            this.selfPE = selfPE;
            this.buildingType = buildingType;


            this.parentORAEOPE = parentORAEOPE;
        }

        public readonly EcsPackedEntity selfPE;
        public readonly DBuildingType buildingType;

        public readonly EcsPackedEntity parentORAEOPE;
    }
}