
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Technology;

namespace SandOcean.Organization
{
    public struct COrganization
    {
        public COrganization(
            EcsPackedEntity selfPE, int selfIndex,
            string organizationName)
        {
            this.selfPE = selfPE;
            this.selfIndex = selfIndex;
            selfName = organizationName;

            ownerPlayerPE = new();

            ownedORAEOPEs = new();

            defaultReserveFleetPE = new();
            reserveFleetPEs = new();
            ownedFleets = new();
            //ownedTaskForces = new();

            technologies = new Dictionary<int, DOrganizationTechnology>[0];
            technologyModifiers = new DTechnologyModifiers(0);

            contentSetIndex = 0;
        }

        public readonly EcsPackedEntity selfPE;
        public readonly int selfIndex;
        public string selfName;

        public EcsPackedEntity ownerPlayerPE;

        public List<EcsPackedEntity> ownedORAEOPEs;

        public EcsPackedEntity defaultReserveFleetPE;
        public List<EcsPackedEntity> reserveFleetPEs;
        public List<EcsPackedEntity> ownedFleets;

        public Dictionary<int, DOrganizationTechnology>[] technologies;
        public DTechnologyModifiers technologyModifiers;
        public int contentSetIndex;
    }
}