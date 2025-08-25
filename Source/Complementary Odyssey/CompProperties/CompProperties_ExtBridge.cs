using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_ExtBridge : CompProperties
    {
        public GraphicData graphicDataSub;
        public TerrainDef deployableTerrain;
        public int ticksPerDeploy = 0;
        public int ticksPerPacking = 0;
        public int ticksPerRecharging = 2500;
        public int maxDeploy = 15;

        public CompProperties_ExtBridge()
        {
            compClass = typeof(CompExtBridge);
        }

        public virtual List<IntVec3> bridgeTiles()
        {
            CellRect rect = new CellRect(0, 1, 1, maxDeploy);
            return rect.Cells.ToList();
        }
    }
}