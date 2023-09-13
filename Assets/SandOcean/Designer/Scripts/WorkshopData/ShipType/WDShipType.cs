
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDShipType : IContentObject, IWorkshopContentObject, IShipType
    {
        public WDShipType(
            string shipTypeName, 
            TaskForceBattleGroup battleGroup)
        {
            this.shipTypeName = shipTypeName;

            gameObjectIndex = -1;
            isValidObject = true;

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

        public int GameObjectIndex 
        { 
            get
            {
                return gameObjectIndex;
            }
            set
            {
                gameObjectIndex = value;
            }
        }
        int gameObjectIndex;

        public bool IsValidObject 
        { 
            get
            {
                return isValidObject;
            }
            set
            {
                isValidObject = value;
            }
        }
        bool isValidObject;

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