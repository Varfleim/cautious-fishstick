
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipPartImprovement
    {
        public SDShipPartImprovement(
            string improvementName)
        {
            this.improvementName = improvementName;
        }

        public string improvementName;
    }
}