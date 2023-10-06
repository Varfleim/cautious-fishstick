
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipPartCoreTechnology
    {
        public SDShipPartCoreTechnology(
            string coreTechnologyName, 
            SDContentObjectLink[] directionsOfImprovement)
        {
            this.coreTechnologyName = coreTechnologyName;

            this.directionsOfImprovement = directionsOfImprovement;
        }

        public string coreTechnologyName;

        public SDContentObjectLink[] directionsOfImprovement;
    }
}