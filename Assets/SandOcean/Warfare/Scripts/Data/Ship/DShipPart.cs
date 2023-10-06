
namespace SandOcean.Warfare.Ship
{
    public class DShipPart : DContentObject, IShipPart
    {
        public DShipPart(
            string shipPartName, 
            ContentObjectLink[] coreTechnologies) : base(shipPartName)
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