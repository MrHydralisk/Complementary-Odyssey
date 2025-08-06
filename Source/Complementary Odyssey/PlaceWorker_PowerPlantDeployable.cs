using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class PlaceWorker_PowerPlantDeployable : PlaceWorker
    {
        public CompProperties_PowerPlantDeployable propsCached;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            if (propsCached == null)
            {
                propsCached = def.GetCompProperties<CompProperties_PowerPlantDeployable>();
            }
            List<IntVec3> tiles = new List<IntVec3>();
            foreach (IntVec3 tile in propsCached.zoneTiles())
            {
                tiles.Add(center + tile.RotatedBy(rot));
            }
            GenDraw.DrawFieldEdges(tiles);
        }
    }
}