
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDContentSetDescription : IContentSetDescription
    {
        public WDContentSetDescription(
            string contentSetName,
            string contentSetVersion,
            string gameVersion,
            string contentSetDirectoryPath)
        {
            this.contentSetName = contentSetName;

            this.contentSetVersion = contentSetVersion;

            this.gameVersion = gameVersion;

            this.contentSetDirectoryPath = contentSetDirectoryPath;
        }

        public string ContentSetName
        {
            get
            {
                return contentSetName;
            }
            set
            {
                contentSetName
                    = value;
            }
        }
        string contentSetName;

        public string ContentSetVersion
        {
            get
            {
                return contentSetVersion;
            }
            set
            {
                contentSetVersion
                    = value;
            }
        }
        string contentSetVersion;

        public string GameVersion
        {
            get
            {
                return gameVersion;
            }
            set
            {
                gameVersion
                    = value;
            }
        }
        string gameVersion;

        public string contentSetDirectoryPath;
    }
}