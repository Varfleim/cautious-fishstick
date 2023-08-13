
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Game
{
    public struct DExtractionEquipment : IContentObject, IGameComponent, IExtractionEquipment
    {
        public DExtractionEquipment(
            string modelName,
            DComponentCoreTechnology[] coreTechnologies,
            float size)
        {
            this.modelName = modelName;

            this.shipClasses = new List<DContentObjectLink>();

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
            extractionSpeed
                = Formulas.ExtractionEquipmentExtractionSpeed(
                    coreTechnologies[0].ModifierValue,
                    size);
        }
    }
}