
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public struct SDShipType
    {
        public SDShipType(
            string typeName,
            string battleGroupName)
        {
            this.typeName = typeName;

            this.battleGroupName = battleGroupName;
        }

        public string typeName;

        public string battleGroupName;
    }
}