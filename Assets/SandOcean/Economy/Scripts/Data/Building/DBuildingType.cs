
using System;

namespace SandOcean.Economy.Building
{
    [Serializable]
    public class DBuildingType : IContentObject, IBuildingType
    {
        public DBuildingType(
            string typeName, 
            BuildingCategory buildingCategory)
        {
            this.typeName = typeName;
            
            this.buildingCategory = buildingCategory;
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

        public BuildingCategory BuildingCategory
        {
            get
            {
                return buildingCategory;
            }
            set
            {
                buildingCategory = value;
            }
        }
        BuildingCategory buildingCategory;
    }
}