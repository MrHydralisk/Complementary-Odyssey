using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class PlaceWorker_VacBarrierRoofPojector : PlaceWorker
    {
        public CompProperties_VacBarrierRoofPojector propsCached;
        public CompVacBarrierRoofPojector compCached;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            List<IntVec3> tiles = new List<IntVec3>();
            if (thing != null)
            {
                if (compCached == null)
                {
                    compCached = thing.TryGetComp<CompVacBarrierRoofPojector>();
                }
                foreach (IntVec3 tile in compCached.barrierTiles())
                {
                    tiles.Add(center + tile.RotatedBy(rot));
                }
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
                    propsCached = thingDef.GetCompProperties<CompProperties_VacBarrierRoofPojector>();
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