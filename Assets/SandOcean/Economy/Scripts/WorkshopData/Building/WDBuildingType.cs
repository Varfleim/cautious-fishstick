
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDBuildingType : IContentObject, IWorkshopContentObject, IBuildingType
    {
        public WDBuildingType(
            string typeName,
            BuildingCategory buildingCategory)
        {
            this.typeName = typeName;
            
            gameObjectIndex = -1;
            isValidObject = true;
            
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