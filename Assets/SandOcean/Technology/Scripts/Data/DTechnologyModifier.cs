
using System;

namespace SandOcean.Technology
{
    [Serializable]
    public struct DTechnologyModifier : ITechnologyModifier
    {
        public DTechnologyModifier(
            TechnologyModifierType modifierType,
            float modifierValue)
        {
            this.modifierType = modifierType;

            this.modifierValue = modifierValue;
        }

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