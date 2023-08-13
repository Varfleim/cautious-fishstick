
using Leopotam.EcsLite;

namespace SandOcean.Player
{
    public struct CPlayer
    {
        public CPlayer(
            EcsPackedEntity selfPE, string selfName)
        {
            this.selfPE = selfPE;
            this.selfName = selfName;

            this.ownedOrganizationPE = new();
        }

        public readonly EcsPackedEntity selfPE;
        public string selfName;

        public EcsPackedEntity ownedOrganizationPE;
    }
}