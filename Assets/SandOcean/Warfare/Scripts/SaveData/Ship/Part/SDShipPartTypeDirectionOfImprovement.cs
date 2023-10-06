
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipPartTypeDirectionOfImprovement
    {
        public SDShipPartTypeDirectionOfImprovement(
            string directionOfImprovementName, 
            SDContentObjectLink[] improvements)
        {
            this.directionOfImprovementName = directionOfImprovementName;
            
            this.improvements = improvements;
        }

        public string directionOfImprovementName;

        public SDContentObjectLink[] improvements;
    }
}