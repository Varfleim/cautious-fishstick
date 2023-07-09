
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDTechnologyComponentCoreModifier
    {
        public SDTechnologyComponentCoreModifier(
            string modifierName, 
            float modifierValue)
        {
            this.modifierName = modifierName;

            this.modifierValue = modifierValue;
        }

        public string modifierName;

        public float modifierValue;
    }
}