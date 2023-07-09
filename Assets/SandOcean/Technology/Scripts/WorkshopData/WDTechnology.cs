
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDTechnology : IContentObject, IWorkshopContentObject, ITechnology
    {
        public WDTechnology(
            string technologyName,
            bool isBaseTechnology,
            WDTechnologyModifier[] technologyModifiers,
            WDTechnologyComponentCoreModifier[] technologyComponentCoreModifiers)
        {
            this.technologyName = technologyName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.isBaseTechnology = isBaseTechnology;

            this.technologyModifiers = technologyModifiers;
            this.technologyComponentCoreModifiers = technologyComponentCoreModifiers;

            this.engines = new List<WDContentObjectRef>();
            this.reactors = new List<WDContentObjectRef>();
            this.fuelTanks = new List<WDContentObjectRef>();
            this.extractionEquipmentSolids = new List<WDContentObjectRef>();
            this.energyGuns = new List<WDContentObjectRef>();
        }

        public string ObjectName 
        { 
            get
            {
                return technologyName;
            }
            set
            {
                technologyName
                    = value;
            }
        }
        string technologyName;

        public int GameObjectIndex
        {
            get
            {
                return gameObjectIndex;
            }
            set
            {
                gameObjectIndex
                    = value;
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
                isValidObject
                    = value;
            }
        }
        bool isValidObject;

        public bool IsBaseTechnology 
        {
            get
            {
                return isBaseTechnology;
            }
            set
            {
                isBaseTechnology
                    = value;
            }
        }
        bool isBaseTechnology;

        public WDTechnologyModifier[] technologyModifiers;
        public WDTechnologyComponentCoreModifier[] technologyComponentCoreModifiers;


        public List<WDContentObjectRef> engines;
        public List<WDContentObjectRef> reactors;
        public List<WDContentObjectRef> fuelTanks;
        public List<WDContentObjectRef> extractionEquipmentSolids;
        public List<WDContentObjectRef> energyGuns;
    }
}