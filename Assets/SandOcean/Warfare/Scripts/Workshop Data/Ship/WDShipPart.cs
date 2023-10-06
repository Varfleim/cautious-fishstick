using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class WDShipPart : WDContentObject, IShipPart
    {
        public WDShipPart(
            string shipPartName,
            WorkshopContentObjectLink[] coreTechnologies) : base(shipPartName)
        {
            this.coreTechnologies = coreTechnologies;
        }

        public ContentObjectLink[] CoreTechnologies
        {
            get
            {
                return coreTechnologies;
            }
            set
            {
                coreTechnologies = value;
            }
        }
        protected ContentObjectLink[] coreTechnologies;
    }
}