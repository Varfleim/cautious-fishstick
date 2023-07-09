
using Leopotam.EcsLite;

namespace SandOcean
{
    public class DHexCellPriority
    {
        public DHexCellPriority(
            EcsPackedEntity cellPE, 
            int cellPriority)
        {
            this.cellPE = cellPE;

            this.cellPriority = cellPriority;
        }

        public EcsPackedEntity cellPE;

        public int cellPriority;

        public DHexCellPriority NextWithSamePriority
        {
            get;
            set;
        }
    }
}