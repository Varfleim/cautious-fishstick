
using Leopotam.EcsLite;

namespace SandOcean.Diplomacy
{
    public struct ROrganizationCreating
    {
        public string organizationName;

        public bool isPlayerOrganization;
        public EcsPackedEntity ownerPlayerPE;
    }
}