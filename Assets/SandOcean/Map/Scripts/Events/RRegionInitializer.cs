
using Leopotam.EcsLite;

namespace SandOcean.Map.Events
{
    public struct RRegionInitializer
    {
        public RRegionInitializer(
            EcsPackedEntity initializedOrganizationPE,
            DContentObjectLink[] effectLinks,
            int minDistanceBetweenInitializers)
        {
            this.initializedOrganizationPE = initializedOrganizationPE;

            this.effectLinks = effectLinks;

            parentInitializerEntity = -1;

            this.minDistanceBetweenInitializers = minDistanceBetweenInitializers;
        }

        public readonly EcsPackedEntity initializedOrganizationPE;

        public readonly DContentObjectLink[] effectLinks;

        public int parentInitializerEntity;

        public readonly int minDistanceBetweenInitializers;
    }
}