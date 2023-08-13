
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDReactor : IContentObject, IWorkshopContentObject, IWorkshopComponent, IReactor
    {
        public WDReactor(
            string modelName,
            WDComponentCoreTechnology[] coreTechnologies,
            float reactorSize,
            float reactorBoost)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<WDContentObjectLink>();

            this.coreTechnologies = coreTechnologies;

            this.reactorSize = reactorSize;
            this.reactorBoost = reactorBoost;

            this.reactorEnergy = 0;
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


        public float ReactorSize
        {
            get
            {
                return reactorSize;
            }
            set
            {
                reactorSize
                    = value;
            }
        }
        float reactorSize;

        public float ReactorBoost
        {
            get
            {
                return reactorBoost;
            }
            set
            {
                reactorBoost
                    = value;
            }
        }
        float reactorBoost;

        public float ReactorEnergy
        {
            get
            {
                return reactorEnergy;
            }
            set
            {
                reactorEnergy
                    = value;
            }
        }
        float reactorEnergy;

        public void CalculateCharacteristics()
        {
            CalculateEnergy();
        }

        public void CalculateEnergy()
        {
            //Если ссылка на технологию энергии на единицу размера действительна
            if (coreTechnologies[0].IsValidLink
                == true)
            {
                reactorEnergy
                    = Formulas.ReactorEnergy(
                        coreTechnologies[0].ModifierValue,
                        reactorSize,
                        reactorBoost);
            }
            //Иначе
            else
            {
                reactorEnergy
                    = 0;
            }
        }
    }
}