
using Leopotam.EcsLite;

namespace SandOcean.Diplomacy
{
    public struct EOrganizationCreating
    {
        public string organizationName;

        public bool isPlayerOrganization;
        public EcsPackedEntity ownerPlayerPE;
    }
}