
using System;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DShipClassComponent : IContentObjectLink
    {
        public DShipClassComponent(
            int contentSetIndex,
            int objectIndex, 
            int numberOfComponents)
        {
            this.contentSetIndex = contentSetIndex;

            this.objectIndex = objectIndex;

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
                return objectIndex;
            }
            set
            {
                objectIndex
                    = value;
            }
        }
        int objectIndex;

        public int numberOfComponents;
    }
}