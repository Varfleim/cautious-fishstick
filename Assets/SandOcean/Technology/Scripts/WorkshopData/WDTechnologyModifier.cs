
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDTechnologyModifier : ITechnologyModifier
    {
        public WDTechnologyModifier(
            string modifierName,
            TechnologyModifierType modifierType, 
            float modifierValue)
        {
            this.modifierName = modifierName;

            this.modifierType = modifierType;

            this.modifierValue = modifierValue;
        }

        public string modifierName;

        public TechnologyModifierType ModifierType 
        { 
            get
            {
                return modifierType;
            }
            set
            {
                modifierType
                    = value;
            }
        }
        TechnologyModifierType modifierType;

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