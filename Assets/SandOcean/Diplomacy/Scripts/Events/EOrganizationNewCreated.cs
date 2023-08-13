
using Leopotam.EcsLite;

namespace SandOcean.Diplomacy
{
    public readonly struct EOrganizationNewCreated
    {
        public EOrganizationNewCreated(
            EcsPackedEntity organizationPE)
        {
            this.organizationPE = organizationPE;
        }

        public readonly EcsPackedEntity organizationPE;
    }
}