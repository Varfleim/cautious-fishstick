
using System;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDTechnologyComponentCoreModifier : ITechnologyComponentCoreModifier
    {
        public WDTechnologyComponentCoreModifier(
            string modifierName,
            TechnologyComponentCoreModifierType modifierType,
            float modifierValue)
        {
            this.modifierName = modifierName;

            this.modifierType = modifierType;

            this.modifierValue = modifierValue;
        }

        public string modifierName;

        public TechnologyComponentCoreModifierType ModifierType
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
        TechnologyComponentCoreModifierType modifierType;

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