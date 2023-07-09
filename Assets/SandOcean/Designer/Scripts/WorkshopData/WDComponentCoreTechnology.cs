
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDComponentCoreTechnology : IContentObjectRef, IWorkshopContentObjectRef, IComponentCoreTechnology
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


            this.contentSetIndex = 0;

            this.technologyIndex = 0;


            this.isValidRef = false;


            this.modifierValue = 0;
        }

        public WDComponentCoreTechnology(
            string modifierName,
            TechnologyComponentCoreModifierType modifierType,
            string contentSetName,
            string technologyName,
            int contentSetIndex,
            int technologyIndex,
            bool isValidRef,
            float modifierValue)
        {
            this.modifierName = modifierName;

            this.modifierType = modifierType;

            this.contentSetName = contentSetName;

            this.technologyName = technologyName;


            this.contentSetIndex = contentSetIndex;

            this.technologyIndex = technologyIndex;


            this.isValidRef = isValidRef;


            this.modifierValue = modifierValue;
        }

        public string modifierName;

        public TechnologyComponentCoreModifierType modifierType;

        public string contentSetName;

        public string technologyName;


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

        public bool IsValidRef
        {
            get
            {
                return isValidRef;
            }
            set
            {
                isValidRef
                    = value;
            }
        }
        bool isValidRef;

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