using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDComponentCoreTechnology
    {
        public SDComponentCoreTechnology(
            string modifierName,
            string contentSetName,
            string technologyName)
        {
            this.modifierName = modifierName;

            this.contentSetName = contentSetName;

            this.technologyName = technologyName;
        }

        public string modifierName;

        public string contentSetName;

        public string technologyName;
    }
}