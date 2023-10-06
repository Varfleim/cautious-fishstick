
namespace SandOcean.Warfare.Ship
{
    public class DShipPartDirectionOfImprovement : DContentObject, IShipPartDirectionOfImprovement
    {
        public DShipPartDirectionOfImprovement(
            string directionOfImprovementName,
            ContentObjectLink[] improvements) : base(directionOfImprovementName)
        {
            this.improvements = improvements;
        }

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