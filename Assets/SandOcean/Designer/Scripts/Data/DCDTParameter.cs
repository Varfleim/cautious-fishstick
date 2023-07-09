
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Designer
{
    [Serializable]
    public struct DCDTParameter
    {
        public string parameterName;

        public float parameterBaseValue;

        public float[] parameterChangeSteps;
    }
}