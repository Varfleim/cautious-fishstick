
using System.Collections.Generic;

namespace SandOcean.Map
{
    public struct CHexRegionGenerationData
    {
        public CHexRegionGenerationData(
            DHexRegionClimate currentClimate, DHexRegionClimate nextClimate)
        {
            this.currentClimate = currentClimate;
            this.nextClimate = nextClimate;
        }

        public DHexRegionClimate currentClimate;
        public DHexRegionClimate nextClimate;

        
    }
}