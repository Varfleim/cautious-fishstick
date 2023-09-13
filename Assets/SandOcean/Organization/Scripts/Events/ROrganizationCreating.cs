
using Leopotam.EcsLite;

namespace SandOcean.Organization
{
    public struct ROrganizationCreating
    {
        public string organizationName;

        public bool isPlayerOrganization;
        public EcsPackedEntity ownerPlayerPE;
    }
}