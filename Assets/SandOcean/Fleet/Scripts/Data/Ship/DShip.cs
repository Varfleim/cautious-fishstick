
using System;

namespace SandOcean.Warfare.Ship
{
    public class DShip : IEquatable<DShip>
    {
        public DShip(
            int selfIndex,
            DShipType shipType)
        {
            this.selfIndex = selfIndex;

            this.shipType = shipType;
        }

        public readonly int selfIndex; 

        public readonly DShipType shipType;

        public bool Equals(DShip other)
        {
            if (other == null)
            {
                return false;
            }

            if (selfIndex == other.selfIndex)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DShip shipObj = obj as DShip;

            if (shipObj == null)
            {
                return false;
            }
            else
            {
                return Equals(shipObj);
            }
        }
    }
}