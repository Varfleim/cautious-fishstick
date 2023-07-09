using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DReactor : IContentObject, IGameComponent, IReactor
    {
        public DReactor(
            string modelName, 
            DComponentCoreTechnology[] coreTechnologies, 
            float reactorSize, 
            float reactorBoost)
        {
            this.modelName = modelName;

            this.shipClasses = new List<DContentObjectRef>();

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


        public List<DContentObjectRef> ShipClasses
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
        List<DContentObjectRef> shipClasses;


        public DComponentCoreTechnology[] coreTechnologies;


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
            reactorEnergy
                = Formulas.EnginePower(
                    coreTechnologies[0].ModifierValue,
                    reactorSize,
                    reactorBoost);
        }
    }
}