
namespace SandOcean.Warfare.Ship
{
    public class DShipClassPart : IShipClassPart
    {
        public DShipClassPart(
            ContentObjectLink part,
            ContentObjectLink coreTechnology,
            ContentObjectLink[] improvements)
        {
            this.part = part;

            this.coreTechnology = coreTechnology;

            this.improvements = improvements;
        }

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