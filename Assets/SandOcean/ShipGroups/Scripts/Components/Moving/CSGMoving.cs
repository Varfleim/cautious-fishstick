
using System.Collections.Generic;

namespace SandOcean.Ship.Moving
{
    public enum ShipGroupMoving : byte
    {
        None,
        Pathfinding,
        Moving,
        Landing,
        Waiting
    }

    public struct CSGMoving
    {
        public CSGMoving(
            int a)
        {
            pathPoints = new();

            mode = ShipGroupMoving.Pathfinding;
        }

        public LinkedList<DShipGroupPathPoint> pathPoints;

        public ShipGroupMoving mode;
    }
}