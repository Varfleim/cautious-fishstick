
namespace SandOcean.Map
{
    public struct DMapArea
    {
        public DMapArea(
            int xMin, int xMax, 
            int zMin, int zMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            
            this.zMin = zMin;
            this.zMax = zMax;
        }

        public int xMin;
        public int xMax;

        public int zMin;
        public int zMax;
    }
}