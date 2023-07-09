
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDShipClassComponent
    {
        public SDShipClassComponent(
            string contentSetName,
            string componentName,
            int numberOfComponents)
        {
            this.contentSetName = contentSetName;

            this.componentName = componentName;

            this.numberOfComponents = numberOfComponents;
        }

        public string contentSetName;

        public string componentName;

        public int numberOfComponents;
    }
}