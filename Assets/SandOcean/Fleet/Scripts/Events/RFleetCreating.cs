
using Leopotam.EcsLite;

namespace SandOcean.Warfare.Fleet
{
    public readonly struct RFleetCreating
    {
        public RFleetCreating(
            EcsPackedEntity ownerOrganizationPE,
            bool isReserve)
        {
            this.ownerOrganizationPE = ownerOrganizationPE;

            this.isReserve = isReserve;
        }

        public readonly EcsPackedEntity ownerOrganizationPE;

        public readonly bool isReserve;
    }
}