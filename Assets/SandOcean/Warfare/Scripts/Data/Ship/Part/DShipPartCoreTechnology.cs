
namespace SandOcean.Warfare.Ship
{
    public class DShipPartCoreTechnology : DContentObject, IShipPartCoreTechnology
    {
        public DShipPartCoreTechnology(
            string coreTechnologyName,
            ContentObjectLink[] directionsOfImprovement) : base(coreTechnologyName)
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