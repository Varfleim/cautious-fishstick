
namespace SandOcean.Map
{
    public struct DHexRegionClimate
    {
        public DHexRegionClimate(
            float clouds, 
            float moisture)
        {
            this.clouds = clouds;
            this.moisture = moisture;
        }

        public float clouds;
        public float moisture;
    }
}