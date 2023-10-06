
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDShipClassComponent : IContentObjectLink
    {
        public WDShipClassComponent(
            string contentSetName,
            string componentName,
            int numberOfComponents)
        {
            this.contentSetName = contentSetName;

            this.componentName = componentName;

            this.numberOfComponents = numberOfComponents;

            this.contentSetIndex = 0;

            this.componentIndex = 0;

            this.isValidLink = false;
        }

        public WDShipClassComponent(
            string contentSetName,
            string componentName,
            int numberOfComponents,
            int contentSetIndex, 
            int componentIndex,
            bool isValidRef)
        {
            this.contentSetName = contentSetName;

            this.componentName = componentName;

            this.contentSetIndex = contentSetIndex;

            this.componentIndex = componentIndex;

            this.numberOfComponents = numberOfComponents;

            this.isValidLink = isValidRef;
        }

        public string contentSetName;

        public string componentName;

        public int numberOfComponents;

        public int ContentSetIndex
        {
            get
            {
                return contentSetIndex;
            }
            set
            {
                contentSetIndex
                    = value;
            }
        }
        int contentSetIndex;

        public int ObjectIndex
        {
            get
            {
                return componentIndex;
            }
            set
            {
                componentIndex
                    = value;
            }
        }
        int componentIndex;

        public bool IsValidLink
        {
            get
            {
                return isValidLink;
            }
            set
            {
                isValidLink
                    = value;
            }
        }
        bool isValidLink;
    }
}