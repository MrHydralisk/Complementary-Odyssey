using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class PlaceWorker_VacResistAOEProjector : PlaceWorker
    {
        public CompProperties_VacResistAOEProjector propsCached;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            List<IntVec3> tiles = new List<IntVec3>();
            if (thing != null && thing.Spawned && thing.PositionHeld == center)
            {
                tiles = thing.TryGetComp<CompVacResistAOEProjector>()?.effectTiles ?? new List<IntVec3>();
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
                    propsCached = thingDef.GetCompProperties<CompProperties_VacResistAOEProjector>();
                }
                if (propsCached != null)
                {
                    tiles = GenRadial.RadialCellsAround(center, propsCached.effectRadius, true).ToList();
                }
            }
            GenDraw.DrawFieldEdges(tiles);
        }
    }
}