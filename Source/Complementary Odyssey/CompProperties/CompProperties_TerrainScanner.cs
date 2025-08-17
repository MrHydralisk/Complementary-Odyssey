using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_TerrainScanner : CompProperties_Scanner
    {
        public int ticksPerScan = 60;
        public int tilesPerScan = 40;

        public CompProperties_TerrainScanner()
        {
            compClass = typeof(CompTerrainScanner);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (compClass == null)
            {
                yield return parentDef.defName + " has CompProperties with null compClass.";
            }
        }
    }
}