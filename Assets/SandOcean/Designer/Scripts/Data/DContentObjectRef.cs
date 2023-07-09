
using System;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DContentObjectRef : IContentObjectRef
    {
        public DContentObjectRef(
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