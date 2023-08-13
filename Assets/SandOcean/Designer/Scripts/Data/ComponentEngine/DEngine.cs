using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DEngine : IContentObject, IGameComponent, IEngine
    {
        public DEngine(
            string modelName, 
            DComponentCoreTechnology[] coreTechnologies,
            float engineSize,
            float engineBoost)
        {
            this.modelName = modelName;

            this.shipClasses = new List<DContentObjectLink>();

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
            enginePower
                = Formulas.EnginePower(
                    coreTechnologies[0].ModifierValue,
                    engineSize,
                    engineBoost);
        }
    }
}