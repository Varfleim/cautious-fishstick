
using System;

namespace SandOcean
{
    [Serializable]
    public struct WDContentObjectLink : IWorkshopContentObjectLink, IContentObjectLink
    {
        public WDContentObjectLink(
            string contentSetName, string objectName)
        {
            this.contentSetName = contentSetName;
            this.objectName = objectName;
            this.isValidLink = false;

            contentSetIndex = -1;
            objectIndex = -1;
        }

        public WDContentObjectLink(
            string contentSetName, string objectName,
            int contentSetIndex, int objectIndex) : this(contentSetName, objectName)
        {
            this.contentSetIndex = contentSetIndex;

            this.objectIndex = objectIndex;
        }

        /// <summary>
        /// ”ƒ¿À»“‹ ”ƒ¿À»“‹ ”ƒ¿À»“‹
        /// </summary>
        /// <param name="contentSetIndex"></param>
        /// <param name="objectIndex"></param>
        public WDContentObjectLink(
            int contentSetIndex, int objectIndex) : this()
        {
            this.contentSetIndex = contentSetIndex;
            this.objectIndex = objectIndex;
        }

        public string ContentSetName
        {
            get
            {
                return contentSetName;
            }
            set
            {
                contentSetName = value;
            }
        }
        string contentSetName;

        public string ObjectName
        {
            get
            {
                return objectName;
            }
            set
            {
                objectName = value;
            }
        }
        string objectName;

        public bool IsValidLink
        {
            get
            {
                return isValidLink;
            }
            set
            {
                isValidLink = value;
            }
        }
        bool isValidLink;

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
        int contentSetIndex;

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
        int objectIndex;
    }

    public class WorkshopContentObjectLink : ContentObjectLink
    {
        public WorkshopContentObjectLink(
            string contentSetName, string objectName) : base(-1, -1)
        {
            this.contentSetName = contentSetName;
            this.objectName = objectName;

            isValidLink = false;
        }

        public string ContentSetName
        {
            get
            {
                return contentSetName;
            }
            set
            {
                contentSetName = value;
            }
        }
        protected string contentSetName;

        public string ObjectName
        {
            get
            {
                return objectName;
            }
            set
            {
                objectName = value;
            }
        }
        protected string objectName;

        public bool IsValidLink
        {
            get
            {
                return isValidLink;
            }
            set
            {
                isValidLink = value;
            }
        }
        protected bool isValidLink;
    }
}