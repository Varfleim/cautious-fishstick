
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDExtractionEquipment
    {
        public SDExtractionEquipment(
            string modelName,
            SDComponentCoreTechnology[] coreTechnologies, 
            float size)
        {
            this.modelName = modelName;

            this.coreTechnologies = coreTechnologies;

            this.size = size;
        }

        public string modelName;

        public SDComponentCoreTechnology[] coreTechnologies;

        public float size;
    }
}