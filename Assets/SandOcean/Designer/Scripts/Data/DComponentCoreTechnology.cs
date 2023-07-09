using System;

namespace SandOcean.Designer.Game
{
    [Serializable]
    public struct DComponentCoreTechnology : IContentObjectRef, IComponentCoreTechnology
    {
        public DComponentCoreTechnology(
            int contentSetIndex,
            int technologyIndex,
            float modifierValue)
        {
            this.contentSetIndex = contentSetIndex;

            this.technologyIndex = technologyIndex;

            this.modifierValue = modifierValue;
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
                return technologyIndex;
            }
            set
            {
                technologyIndex
                    = value;
            }
        }
        int technologyIndex;

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