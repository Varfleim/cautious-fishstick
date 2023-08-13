
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDEngine : IContentObject, IWorkshopContentObject, IWorkshopComponent, IEngine
    {
        public WDEngine(
            string modelName,
            WDComponentCoreTechnology[] coreTechnologies,
            float engineSize,
            float engineBoost)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<WDContentObjectLink>();

            this.coreTechnologies = coreTechnologies;

            this.engineSize = engineSize;
            this.engineBoost = engineBoost;

            this.enginePower = 0;
        }

        public string ObjectName
        {
            get
            {
                return modelName;
            }
            set
            {
                modelName
                    = value;
            }
        }
        string modelName;


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

        public List<WDContentObjectLink> ShipClasses
        {
            get
            {
                return shipClasses;
            }
            set
            {
                shipClasses
                    = value;
            }
        }
        List<WDContentObjectLink> shipClasses;


        public WDComponentCoreTechnology[] coreTechnologies;


        public float EngineSize 
        {
            get
            {
                return engineSize;
            }
            set
            {
                engineSize
                    = value;
            }
        }
        float engineSize;

        public float EngineBoost 
        {
            get
            {
                return engineBoost;
            }
            set
            {
                engineBoost
                    = value;
            }
        }
        float engineBoost;

        public float EnginePower 
        {
            get
            {
                return enginePower;
            }
            set
            {
                enginePower
                    = value;
            }
        }
        float enginePower;

        public void CalculateCharacteristics()
        {
            CalculatePower();
        }

        public void CalculatePower()
        {
            //Если ссылка на технологию мощности на единицу размера действительна
            if (coreTechnologies[0].IsValidLink
                == true)
            {
                enginePower
                    = Formulas.EnginePower(
                        coreTechnologies[0].ModifierValue,
                        engineSize,
                        engineBoost);
            }
            //Иначе
            else
            {
                enginePower
                    = 0;
            }
        }
    }
}