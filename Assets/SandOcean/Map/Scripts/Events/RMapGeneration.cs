namespace SandOcean.Map.Events
{
    public readonly struct RMapGeneration
    {
        public RMapGeneration(
            int chunkCountX, int chunkCountZ,
            int subdivisions)
        {
            this.chunkCountX = chunkCountX;
            this.chunkCountZ = chunkCountZ;

            this.subdivisions = subdivisions;
        }

        public readonly int chunkCountX;
        public readonly int chunkCountZ;

        public readonly int subdivisions;
    }
}