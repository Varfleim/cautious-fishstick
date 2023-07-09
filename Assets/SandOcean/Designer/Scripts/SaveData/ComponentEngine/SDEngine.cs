using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDEngine
    {
        public SDEngine(
            string modelName,
            SDComponentCoreTechnology[] coreTechnologies,
            float engineSize,
            float engineBoost)
        {
            this.modelName = modelName;

            this.coreTechnologies = coreTechnologies;

            this.engineSize = engineSize;
            this.engineBoost = engineBoost;
        }

        public string modelName;

        public SDComponentCoreTechnology[] coreTechnologies;

        public float engineSize;
        public float engineBoost;
    }
}