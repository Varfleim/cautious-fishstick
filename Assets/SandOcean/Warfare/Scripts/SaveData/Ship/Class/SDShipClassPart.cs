
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipClassPart
    {
        public SDShipClassPart(
            SDContentObjectLink part, 
            SDContentObjectLink coreTechnology, 
            SDContentObjectLink[] improvements)
        {
            this.part = part;
            
            this.coreTechnology = coreTechnology;
            
            this.improvements = improvements;
        }

        public SDContentObjectLink part;

        public SDContentObjectLink coreTechnology;

        public SDContentObjectLink[] improvements;
    }
}