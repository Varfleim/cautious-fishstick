
using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public readonly struct EEcORAEONewCreated
    {
        public EEcORAEONewCreated(
            EcsPackedEntity organizationPE, 
            EcsPackedEntity oLAEOPE)
        {
            this.organizationPE = organizationPE;

            this.oLAEOPE = oLAEOPE;
        }

        public readonly EcsPackedEntity organizationPE;

        public readonly EcsPackedEntity oLAEOPE;
    }
}