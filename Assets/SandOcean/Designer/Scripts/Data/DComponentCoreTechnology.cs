using System;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DComponentCoreTechnology : IComponentCoreTechnology
    {
        public DComponentCoreTechnology(
            DContentObjectLink contentObjectLink,
            float modifierValue)
        {
            this.contentObjectLink = contentObjectLink;

            this.modifierValue = modifierValue;
        }

        public DContentObjectLink ContentObjectLink
        {
            get
            {
                return contentObjectLink;
            }
            set
            {
                contentObjectLink = value;
            }
        }
        DContentObjectLink contentObjectLink;

        public float ModifierValue
        {
            get
            {
                return modifierValue;
            }
            set
            {
                modifierValue
                    = value;
            }
        }
        float modifierValue;
    }
}