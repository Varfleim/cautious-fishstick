
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDContentSetDescription
    {
        public SDContentSetDescription(
            string contentSetName,
            string contentSetVersion,
            string gameVersion)
        {
            this.contentSetName = contentSetName;

            this.contentSetVersion = contentSetVersion;

            this.gameVersion = gameVersion;
        }

        public string contentSetName;

        public string contentSetVersion;

        public string gameVersion;
    }

    [Serializable]
    public class SDContentSetDescriptionClass
    {
        public SDContentSetDescription contentSetDescription;
    }
}