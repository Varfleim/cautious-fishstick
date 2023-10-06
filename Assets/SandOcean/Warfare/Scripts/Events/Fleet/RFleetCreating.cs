
using Leopotam.EcsLite;

namespace SandOcean.Warfare.Fleet
{
    public readonly struct RFleetCreating
    {
        public RFleetCreating(
            EcsPackedEntity parentOrganizationPE,
            bool isReserve)
        {
            this.parentOrganizationPE = parentOrganizationPE;

            this.isReserve = isReserve;
        }

        public readonly EcsPackedEntity parentOrganizationPE;

        public readonly bool isReserve;
    }
}