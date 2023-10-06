
using System;

namespace SandOcean.Warfare.Ship
{
    [Serializable]
    public class DShipType : IContentObject, IShipType
    {
        public DShipType(
            string typeName,
            TaskForceBattleGroup battleGroup)
        {
            this.typeName = typeName;

            this.battleGroup = battleGroup;
        }

        public string ObjectName
        {
            get
            {
                return typeName;
            }
            set
            {
                typeName = value;
            }
        }
        string typeName;

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