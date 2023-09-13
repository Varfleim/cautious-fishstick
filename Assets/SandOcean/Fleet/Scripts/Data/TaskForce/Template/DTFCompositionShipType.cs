
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class DTFCompositionShipType
    {
        public DTFCompositionShipType(
            DShipType shipType,
            int requestedShipCount)
        {
            this.shipType = shipType;

            this.requestedShipCount = requestedShipCount;

            sentedShipCount = 0;
        }

        public DShipType shipType;

        public int requestedShipCount;
        public int sentedShipCount;
    }
}