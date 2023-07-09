
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Designer
{
    [Serializable]
    public struct DComponentDesignerType
    {
        public string typeName;

        public ShipComponentType type;

        public DCDTParameter[] typeParameters;
    }
}