
using UnityEngine;

namespace SandOcean.Map
{
    public struct DHexHash
    {
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;

        public static DHexHash Create()
        {
            DHexHash hash;

            hash.a = Random.value * 0.999f;
            hash.b = Random.value * 0.999f;
            hash.c = Random.value * 0.999f;
            hash.d = Random.value * 0.999f;
            hash.e = Random.value * 0.999f;

            return hash;
        }
    }
}