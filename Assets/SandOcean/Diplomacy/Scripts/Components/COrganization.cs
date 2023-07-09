
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Technology;

namespace SandOcean.Diplomacy
{
    public struct COrganization
    {
        public COrganization(
            EcsPackedEntity selfPE, int selfIndex,
            string organizationName)
        {
            this.selfPE = selfPE;
            this.selfIndex = selfIndex;
            this.selfName = organizationName;

            this.ownerPlayerPE = new();

            ownedORAEOPEs = new();

            this.technologies = new Dictionary<int, DOrganizationTechnology>[0];
            this.technologyModifiers = new DTechnologyModifiers(0);

            this.contentSetIndex = 0;
        }

        public readonly EcsPackedEntity selfPE;
        public readonly int selfIndex;
        public string selfName;

        public EcsPackedEntity ownerPlayerPE;

        public List<EcsPackedEntity> ownedORAEOPEs;

        public Dictionary<int, DOrganizationTechnology>[] technologies;
        public DTechnologyModifiers technologyModifiers;

        public int contentSetIndex;
    }
}