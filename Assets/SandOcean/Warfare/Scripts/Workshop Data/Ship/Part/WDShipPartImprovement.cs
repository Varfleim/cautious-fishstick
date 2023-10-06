
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class WDShipPartImprovement : WDContentObject, IShipPartImprovement
    {
        public WDShipPartImprovement(
            string improvementName) : base(improvementName)
        {

        }
    }
}