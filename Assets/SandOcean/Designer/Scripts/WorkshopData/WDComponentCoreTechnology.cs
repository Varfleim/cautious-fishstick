
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDComponentCoreTechnology : IWorkshopContentObjectLink, IComponentCoreTechnology
    {
        public WDComponentCoreTechnology(
            string modifierName,
            TechnologyComponentCoreModifierType modifierType,
            string contentSetName,
            string technologyName)
        {
            this.modifierName = modifierName;

            this.modifierType = modifierType;

            this.contentSetName = contentSetName;

            this.technologyName = technologyName;


            contentObjectLink = new();

            isValidLink = false;


            modifierValue = 0;
        }

        public WDComponentCoreTechnology(
            string modifierName,
            TechnologyComponentCoreModifierType modifierType,
            string contentSetName,
            string technologyName,
            DContentObjectLink contentObjectLink,
            bool isValidLink,
            float modifierValue)
        {
            this.modifierName = modifierName;

            this.modifierType = modifierType;

            this.contentSetName = contentSetName;

            this.technologyName = technologyName;


            this.contentObjectLink = contentObjectLink;

            this.isValidLink = isValidLink;


            this.modifierValue = modifierValue;
        }

        public string modifierName;

        public TechnologyComponentCoreModifierType modifierType;

        public string contentSetName;

        public string technologyName;


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

        public bool IsValidLink
        {
            get
            {
                return isValidLink;
            }
            set
            {
                isValidLink
                    = value;
            }
        }
        bool isValidLink;

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