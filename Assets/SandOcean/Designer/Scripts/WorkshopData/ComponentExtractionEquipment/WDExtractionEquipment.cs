
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDExtractionEquipment : IContentObject, IWorkshopContentObject, IWorkshopComponent, IExtractionEquipment
    {
        public WDExtractionEquipment(
            string modelName,
            WDComponentCoreTechnology[] coreTechnologies,
            float size)
        {
            this.modelName = modelName;

            this.gameObjectIndex = -1;
            this.isValidObject = true;

            this.shipClasses = new List<WDContentObjectRef>();

            this.coreTechnologies = coreTechnologies;

            this.size = size;

            this.extractionSpeed = 0;
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
                return size;
            }
            set
            {
                size
                    = value;
            }
        }
        float size;

        public float ExtractionSpeed
        {
            get
            {
                return extractionSpeed;
            }
            set
            {
                extractionSpeed
                    = value;
            }
        }
        float extractionSpeed;

        public void CalculateCharacteristics()
        {
            CalculateExtractionSpeed();
        }

        public void CalculateExtractionSpeed()
        {
            //≈сли ссылка на технологию скорости добычи на единицу размера действительна
            if (coreTechnologies[0].IsValidRef
                == true)
            {
                extractionSpeed
                    = Formulas.ExtractionEquipmentExtractionSpeed(
                        coreTechnologies[0].ModifierValue,
                        size);
            }
            //»наче
            else
            {
                extractionSpeed
                    = 0;
            }
        }
    }
}