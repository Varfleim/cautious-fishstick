using System;

namespace SandOcean
{
    [Serializable]
    public struct DContentSetDescription : IContentSetDescription
    {
        public DContentSetDescription(
            string contentSetName,
            string contentSetVersion, 
            string gameVersion)
        {
            this.contentSetName = contentSetName;

            this.contentSetVersion = contentSetVersion;

            this.gameVersion = gameVersion;
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
    }
}