
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDShipClassComponent : IContentObjectRef, IWorkshopContentObjectRef
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

            this.isValidRef = false;
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

            this.isValidRef = isValidRef;
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

        public bool IsValidRef
        {
            get
            {
                return isValidRef;
            }
            set
            {
                isValidRef
                    = value;
            }
        }
        bool isValidRef;
    }
}