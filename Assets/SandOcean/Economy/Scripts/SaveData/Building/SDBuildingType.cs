
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDBuildingType
    {
        public SDBuildingType(
            string buildingTypeName, 
            string buildingCategory)
        {
            this.typeName = buildingTypeName;
            
            this.buildingCategory = buildingCategory;
        }

        public string typeName;

        public string buildingCategory;
    }
}