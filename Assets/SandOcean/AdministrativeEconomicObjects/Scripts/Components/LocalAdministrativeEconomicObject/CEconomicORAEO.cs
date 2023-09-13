
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public struct CEconomicORAEO
    {
        public CEconomicORAEO(
            EcsPackedEntity selfPE)
        {
            this.selfPE = selfPE;
        }

        public readonly EcsPackedEntity selfPE;
    }
}