using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDReactor
    {
        public SDReactor(
            string modelName,
            SDComponentCoreTechnology[] coreTechnologies,
            float reactorSize,
            float reactorBoost)
        {
            this.modelName = modelName;

            this.coreTechnologies = coreTechnologies;

            this.reactorSize = reactorSize;
            this.reactorBoost = reactorBoost;
        }

        public string modelName;

        public SDComponentCoreTechnology[] coreTechnologies;

        public float reactorSize;
        public float reactorBoost;
    }
}