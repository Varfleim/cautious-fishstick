
using System;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DShipClassComponent : IContentObjectRef
    {
        public DShipClassComponent(
            int contentSetIndex,
            int componentIndex, 
            int numberOfComponents)
        {
            this.contentSetIndex = contentSetIndex;

            this.componentIndex = componentIndex;

            this.numberOfComponents = numberOfComponents;
        }

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

        public int numberOfComponents;
    }
}