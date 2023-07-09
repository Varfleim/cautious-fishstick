
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDHoldFuelTank : IContentObject, IWorkshopContentObject, IWorkshopComponent, IHold, IHoldFuelTank
    {
        public WDHoldFuelTank(
            string modelName,
            WDComponentCoreTechnology[] coreTechnologies, 
            float fuelTankSize)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<WDContentObjectRef>();

            this.coreTechnologies = coreTechnologies;

            this.fuelTankSize = fuelTankSize;

            this.fuelTankCapacity = 0;
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


        public List<WDContentObjectRef> ShipClasses
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
        List<WDContentObjectRef> shipClasses;


        public WDComponentCoreTechnology[] coreTechnologies;


        public float Size
        {
            get
            {
                return fuelTankSize;
            }
            set
            {
                fuelTankSize
                    = value;
            }
        }
        float fuelTankSize;


        public float Capacity
        {
            get
            {
                return fuelTankCapacity;
            }
            set
            {
                fuelTankCapacity
                    = value;
            }
        }
        float fuelTankCapacity;

        public void CalculateCharacteristics()
        {
            CalculateCapacity();
        }

        public void CalculateCapacity()
        {
            //Если ссылка на технологию сжатия топлива верна
            if (coreTechnologies[0].IsValidRef
                == true)
            {
                fuelTankCapacity
                    = Formulas.FuelTankCapacity(
                        coreTechnologies[0].ModifierValue,
                        fuelTankSize);
            }
            //Иначе
            else
            {
                fuelTankCapacity
                    = 0;
            }
        }
    }
}