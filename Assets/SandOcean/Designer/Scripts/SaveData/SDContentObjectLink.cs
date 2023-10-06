
using System;

namespace SandOcean
{
    [Serializable]
    public struct SDContentObjectLink
    {
        public SDContentObjectLink(
            string contentSetName, 
            string objectName)
        {
            this.contentSetName = contentSetName;
            
            this.objectName = objectName;
        }

        public string contentSetName;

        public string objectName;        
    }
}