using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class PlaceWorker_VacBarrierRoofProjector : PlaceWorker
    {
        public CompProperties_VacBarrierRoofProjector propsCached;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            List<IntVec3> tiles = new List<IntVec3>();
            if (thing != null)
            {
                tiles = thing.TryGetComp<CompVacBarrierRoofProjector>()?.barrierTilesRotated() ?? new List<IntVec3>();
            }
            else
            {
                ThingDef thingDef = def.entityDefToBuild as ThingDef;
                if (thingDef == null)
                {
                    thingDef = def;
                }
                if (propsCached == null)
                {
                    propsCached = thingDef.GetCompProperties<CompProperties_VacBarrierRoofProjector>();
                }
                if (propsCached != null)
                {
                    foreach (IntVec3 tile in propsCached.barrierTiles())
                    {
                        tiles.Add(center + tile.RotatedBy(rot));
                    }
                }
            }
            GenDraw.DrawFieldEdges(tiles);
        }
    }
}