
using Leopotam.EcsLite;

namespace SandOcean
{
    public class DHexRegionPriority
    {
        public DHexRegionPriority(
            EcsPackedEntity regionPE, 
            int regionPriority)
        {
            this.regionPE = regionPE;

            this.regionPriority = regionPriority;
        }

        public DHexRegionPriority(
            EcsPackedEntity regionPE,
            float regionPriority)
        {
            this.regionPE = regionPE;

            this.regionPriority2 = regionPriority;
        }

        public EcsPackedEntity regionPE;

        public int regionPriority;
        public float regionPriority2;

        public DHexRegionPriority NextWithSamePriority
        {
            get;
            set;
        }
    }
}