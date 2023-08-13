
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

            this.engines = new List<WDContentObjectLink>();
            this.reactors = new List<WDContentObjectLink>();
            this.fuelTanks = new List<WDContentObjectLink>();
            this.extractionEquipmentSolids = new List<WDContentObjectLink>();
            this.energyGuns = new List<WDContentObjectLink>();
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


        public List<WDContentObjectLink> engines;
        public List<WDContentObjectLink> reactors;
        public List<WDContentObjectLink> fuelTanks;
        public List<WDContentObjectLink> extractionEquipmentSolids;
        public List<WDContentObjectLink> energyGuns;
    }
}