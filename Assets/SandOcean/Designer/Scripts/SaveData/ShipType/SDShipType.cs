
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDShipType
    {
        public SDShipType(
            string typeName, 
            string battleGroupName)
        {
            this.shipTypeName = typeName;
            
            this.battleGroupName = battleGroupName;
        }

        public string shipTypeName;

        public string battleGroupName;
    }
}