
using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public enum ORAEOType : byte
    {
        Exploration,
        Economic
    }

    public struct DOrganizationRAEO
    {
        public DOrganizationRAEO(
            EcsPackedEntity organizationLAEOPE,
            ORAEOType oLAEOType)
        {
            this.organizationRAEOPE = organizationLAEOPE;

            this.organizationRAEOType = oLAEOType;
        }

        public readonly EcsPackedEntity organizationRAEOPE;

        public ORAEOType organizationRAEOType;
    }
}