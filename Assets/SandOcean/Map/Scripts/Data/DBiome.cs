
namespace SandOcean.Map
{
    public struct DBiome
    {
        public DBiome(
            int terrain,
            int plant)
        {
            this.terrainTypeIndex = terrain;

            this.plant = plant;
        }

        public int terrainTypeIndex;

        public int plant;
    }
}