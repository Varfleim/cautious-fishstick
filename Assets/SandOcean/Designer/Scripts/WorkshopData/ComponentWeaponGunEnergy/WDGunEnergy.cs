
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDGunEnergy : IContentObject, IWorkshopContentObject, IWorkshopComponent, IGun, IGunEnergy
    {
        public WDGunEnergy(
            string modelName, 
            WDComponentCoreTechnology[] coreTechnologies, 
            float gunCaliber, 
            float gunBarrelLength)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<WDContentObjectRef>();

            this.coreTechnologies = coreTechnologies;

            this.gunCaliber = gunCaliber;
            this.gunBarrelLength = gunBarrelLength;

            this.size = 0;
            this.gunEnergyDamage = 0;
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


        public float GunCaliber
        {
            get
            {
                return gunCaliber;
            }
            set
            {
                gunCaliber
                    = value;
            }
        }
        float gunCaliber;

        public float GunBarrelLength
        {
            get
            {
                return gunBarrelLength;
            }
            set
            {
                gunBarrelLength
                    = value;
            }
        }
        float gunBarrelLength;

        public float Size
        {
            get
            {
                return size;
            }
            set
            {
                size
                    = value;
            }
        }
        float size;


        public float GunEnergyDamage
        {
            get
            {
                return gunEnergyDamage;
            }
            set
            {
                gunEnergyDamage
                    = value;
            }
        }
        float gunEnergyDamage;

        public void CalculateCharacteristics()
        {
            CalculateSize();

            CalculateDamage();
        }

        public void CalculateSize()
        {
            size
                = Formulas.GunSizeCalculate(
                    gunCaliber,
                    gunBarrelLength);
        }

        public void CalculateDamage()
        {
            gunEnergyDamage
                = Formulas.EnergyGunDamageCalculate(
                    gunCaliber,
                    gunBarrelLength);
        }
    }
}