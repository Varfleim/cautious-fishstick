
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Game
{
    public struct DHoldFuelTank : IContentObject, IGameComponent, IHold, IHoldFuelTank
    {
        public DHoldFuelTank(
            string modelName,
            DComponentCoreTechnology[] coreTechnologies,
            float fuelTankSize)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<DContentObjectLink>();

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


        public List<DContentObjectLink> ShipClasses
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
        List<DContentObjectLink> shipClasses;


        public DComponentCoreTechnology[] coreTechnologies;


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
            fuelTankCapacity
                = Formulas.FuelTankCapacity(
                    coreTechnologies[0].ModifierValue,
                    fuelTankSize);
        }
    }
}