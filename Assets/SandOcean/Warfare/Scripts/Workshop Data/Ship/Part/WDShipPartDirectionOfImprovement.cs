
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class WDShipPartDirectionOfImprovement : WDContentObject, IShipPartDirectionOfImprovement
    {
        public WDShipPartDirectionOfImprovement(
            string directionOfImprovementName,
            WorkshopContentObjectLink[] improvements) : base(directionOfImprovementName)
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