
using System;

namespace SandOcean.Technology
{
    [Serializable]
    public struct DTechnologyComponentCoreModifier : ITechnologyComponentCoreModifier
    {
        public DTechnologyComponentCoreModifier(
            TechnologyComponentCoreModifierType modifierType, 
            float modifierValue)
        {
            this.modifierType = modifierType;

            this.modifierValue = modifierValue;
        }

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