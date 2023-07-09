
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public struct CEconomicORAEO
    {
        public CEconomicORAEO(
            EcsPackedEntity selfPE)
        {
            this.selfPE = selfPE;

            landingShipGroups = new();
            landedShipGroups = new();
        }

        public readonly EcsPackedEntity selfPE;

        public List<EcsPackedEntity> landingShipGroups;
        public LinkedList<EcsPackedEntity> landedShipGroups;
    }
}