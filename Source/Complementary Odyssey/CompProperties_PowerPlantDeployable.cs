using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_PowerPlantDeployable : CompProperties_Power
    {
        public GraphicData graphicDataSub;
        public ThingDef deployableThing;
        public int ticksPerDeploy = 0;
        public int ticksPerPacking = 0;
        public float powerOutputVacMult = 0;
        public IntVec2 zoneSize = IntVec2.One;
        public List<IntVec3> zoneAdditionalTiles = new List<IntVec3>();
        public IntVec3 zoneOffset = IntVec3.Zero;

        public virtual List<IntVec3> zoneTiles()
        {
            List<IntVec3> tiles = new CellRect(zoneOffset.x, zoneOffset.z, zoneSize.x, zoneSize.z).Cells.ToList();
            tiles.AddRange(zoneAdditionalTiles);
            return tiles;
        }
    }
}