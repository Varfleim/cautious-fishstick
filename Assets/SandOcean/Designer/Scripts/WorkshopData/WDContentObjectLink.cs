
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDContentObjectLink : IContentObjectLink
    {
        public WDContentObjectLink(
            int contentSetIndex,
            int objectIndex)
        {
            this.contentSetIndex = contentSetIndex;

            this.objectIndex = objectIndex;
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
    }
}