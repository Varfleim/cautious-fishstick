
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDTechnology
    {
        public SDTechnology(
            string technologyName,
            bool isBaseTechnology,
            SDTechnologyModifier[] technologyModifiers,
            SDTechnologyComponentCoreModifier[] componentCoreModifiers)
        {
            this.technologyName = technologyName;

            this.isBaseTechnology = isBaseTechnology;

            this.technologyModifiers = technologyModifiers;
            technologyComponentCoreModifiers = componentCoreModifiers;
        }

        public string technologyName;

        public bool isBaseTechnology;

        public SDTechnologyModifier[] technologyModifiers;
        public SDTechnologyComponentCoreModifier[] technologyComponentCoreModifiers;
    }

    [Serializable]
    public class SDTechnologyClass
    {
        public SDTechnology[] technologies;
    }
}