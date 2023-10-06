
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipPart
    {
        public SDShipPart(
            string partName,
            SDContentObjectLink[] coreTechnologies)
        {
            this.partName = partName;

            this.coreTechnologies = coreTechnologies;
        }

        public string partName;

        public SDContentObjectLink[] coreTechnologies;
    }
}