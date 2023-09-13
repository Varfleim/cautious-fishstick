namespace SandOcean.Warfare.Ship
{
    public struct DCountedShipType
    {
        public DCountedShipType(
            DShipType shipType,
            int shipCount)
        {
            this.shipType = shipType;

            this.shipCount = shipCount;
        }

        public DShipType shipType;

        public int shipCount;
    }
}