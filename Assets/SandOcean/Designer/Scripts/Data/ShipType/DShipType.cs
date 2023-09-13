
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class DShipType : IContentObject, IShipType
    {
        public DShipType(
            string shipTypeName,
            TaskForceBattleGroup battleGroup)
        {
            this.shipTypeName = shipTypeName;

            this.battleGroup = battleGroup;
        }

        public string ObjectName
        {
            get
            {
                return shipTypeName;
            }
            set
            {
                shipTypeName = value;
            }
        }
        string shipTypeName;

        public TaskForceBattleGroup BattleGroup
        {
            get
            {
                return battleGroup;
            }
            set
            {
                battleGroup = value;
            }
        }
        TaskForceBattleGroup battleGroup;
    }
}