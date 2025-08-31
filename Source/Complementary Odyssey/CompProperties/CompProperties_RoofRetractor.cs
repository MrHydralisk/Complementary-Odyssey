using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_RoofRetractor : CompProperties
    {
        public GraphicData graphicDataSub;
        public int ticksPerDeploy = 0;
        public int ticksPerPacking = 0;
        public int ticksPerRecharging = 2500;
        public IntVec2 Borders = new IntVec2(1, 15);

        public CompProperties_RoofRetractor()
        {
            compClass = typeof(CompRoofRetractor);
        }

        public virtual List<IntVec3> roofTiles(IntVec2 borders = new IntVec2())
        {
            if (borders == new IntVec2())
            {
                borders = Borders;
            }
            CellRect rect = new CellRect(0, borders.x, 1, borders.z - borders.x + 1);
            return rect.Cells.ToList();
        }
    }
}