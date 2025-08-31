using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_CompressedSoil : CompProperties
    {
        public TerrainDef terrainDef;
        public int ticksPerCompression = 300000;

        public CompProperties_CompressedSoil()
        {
            compClass = typeof(CompCompressedSoil);
        }
    }
}