
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public struct CRegionAEO
    {
        public CRegionAEO(
            EcsPackedEntity selfPE)
        {
            this.selfPE = selfPE;

            ownerOrganizationPE = new();
            ownerOrganizationIndex = -1;

            organizationRAEOs = new DOrganizationRAEO[0];

            landingShipGroups = new();
            landedShipGroups = new();
        }

        public readonly EcsPackedEntity selfPE;

        public EcsPackedEntity ownerOrganizationPE;
        public int ownerOrganizationIndex;

        public DOrganizationRAEO[] organizationRAEOs;

        public List<EcsPackedEntity> landingShipGroups;
        public LinkedList<EcsPackedEntity> landedShipGroups;
    }
}