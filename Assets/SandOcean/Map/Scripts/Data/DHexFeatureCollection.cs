using System;

using UnityEngine;

namespace SandOcean.Map
{
    [Serializable]
    public struct DHexFeatureCollection
    {
        public Transform[] prefabs;

        public Transform Pick(
            float choice)
        {
            return prefabs[(int)(choice * prefabs.Length)];
        }
    }
}