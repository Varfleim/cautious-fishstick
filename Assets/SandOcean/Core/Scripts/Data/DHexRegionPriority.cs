
using Leopotam.EcsLite;

namespace SandOcean
{
    public class DHexRegionPriority
    {
        public DHexRegionPriority(
            EcsPackedEntity cellPE, 
            int cellPriority)
        {
            this.cellPE = cellPE;

            this.cellPriority = cellPriority;
        }

        public EcsPackedEntity cellPE;

        public int cellPriority;

        public DHexRegionPriority NextWithSamePriority
        {
            get;
            set;
        }
    }
}