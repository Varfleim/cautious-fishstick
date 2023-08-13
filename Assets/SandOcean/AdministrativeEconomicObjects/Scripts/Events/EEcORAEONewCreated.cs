
using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public readonly struct EEcORAEONewCreated
    {
        public EEcORAEONewCreated(
            EcsPackedEntity organizationPE, 
            EcsPackedEntity oRAEOPE)
        {
            this.organizationPE = organizationPE;

            this.oRAEOPE = oRAEOPE;
        }

        public readonly EcsPackedEntity organizationPE;

        public readonly EcsPackedEntity oRAEOPE;
    }
}