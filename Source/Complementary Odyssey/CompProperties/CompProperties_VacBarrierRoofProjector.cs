using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_VacBarrierRoofProjector : CompProperties_Power
    {
        public IntVec2 maxBarrierSize = new IntVec2(5, 5);
        public IntVec3 maxBarrierOffset = new IntVec3(0, -4, 1);
        public IntVec2 initialBarrierSize = new IntVec2(5, 3);
        public IntVec2 initialBarrierOffset = new IntVec2(-2, 1);
        public int maxArea = 16;

        public CompProperties_VacBarrierRoofProjector()
        {
            compClass = typeof(CompVacBarrierRoofProjector);
        }

        public List<IntVec3> barrierTiles()
        {
            return new CellRect(initialBarrierOffset.x, initialBarrierOffset.z, initialBarrierSize.x, initialBarrierSize.z).Cells.ToList();
        }
    }
}