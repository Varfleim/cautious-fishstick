
using System;

namespace SandOcean
{
    [Serializable]
    public struct DContentObjectLink : IContentObjectLink
    {
        public DContentObjectLink(
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

    public class ContentObjectLink
    {
        public ContentObjectLink(
            int contentSetIndex, int objectIndex)
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
                contentSetIndex = value;
            }
        }
        protected int contentSetIndex;

        public int ObjectIndex
        {
            get
            {
                return objectIndex;
            }
            set
            {
                objectIndex = value;
            }
        }
        protected int objectIndex;
    }
}