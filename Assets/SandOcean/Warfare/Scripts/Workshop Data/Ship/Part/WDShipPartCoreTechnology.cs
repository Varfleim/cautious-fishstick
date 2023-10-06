using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class WDShipPartCoreTechnology : WDContentObject, IShipPartCoreTechnology
    {
        public WDShipPartCoreTechnology(
            string coreTechnologyName,
            WorkshopContentObjectLink[] directionsOfImprovement) : base(coreTechnologyName)
        {
            this.directionsOfImprovement = directionsOfImprovement;
        }

        public ContentObjectLink[] DirectionsOfImprovement
        {
            get
            {
                return directionsOfImprovement;
            }
            set
            {
                directionsOfImprovement = value;
            }
        }
        protected ContentObjectLink[] directionsOfImprovement;
    }
}