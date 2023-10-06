using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class WDShipClassPart : IShipClassPart
    {
        public WDShipClassPart(
            WorkshopContentObjectLink part,
            WorkshopContentObjectLink coreTechnology,
            WorkshopContentObjectLink[] improvements)
        {
            isValidLink = false;

            this.part = part;
            
            this.coreTechnology = coreTechnology;
            
            this.improvements = improvements;
        }

        public bool IsValidLink
        {
            get
            {
                return isValidLink;
            }
            set
            {
                isValidLink = value;
            }
        }
        protected bool isValidLink;

        public ContentObjectLink Part
        {
            get
            {
                return part;
            }
            set
            {
                part = value;
            }
        }
        protected ContentObjectLink part;

        public ContentObjectLink CoreTechnology
        {
            get
            {
                return coreTechnology;
            }
            set
            {
                coreTechnology = value;
            }
        }
        protected ContentObjectLink coreTechnology;

        public ContentObjectLink[] Improvements
        {
            get
            {
                return improvements;
            }
            set
            {
                improvements = value;
            }
        }
        protected ContentObjectLink[] improvements;
    }
}