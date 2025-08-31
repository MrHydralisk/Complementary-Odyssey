using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompVacResistAOEProjector : ThingComp
    {
        public CompProperties_VacResistAOEProjector Props => props as CompProperties_VacResistAOEProjector;

        public MapComponent_CompOdyssey compOdysseyMapComponent => compOdysseyMapComponentCached ?? (compOdysseyMapComponentCached = parent.MapHeld.GetComponent<MapComponent_CompOdyssey>() ?? null);
        private MapComponent_CompOdyssey compOdysseyMapComponentCached;

        public List<IntVec3> effectTiles
        {
            get
            {
                if (effectTilesCached.NullOrEmpty())
                {
                    effectTilesCached = GenRadial.RadialCellsAround(parent.PositionHeld, Props.effectRadius, true).ToList();
                }
                return effectTilesCached;
            }
        }
        public List<IntVec3> effectTilesCached;

        public bool isWasActive;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                compOdysseyMapComponentCached = null;
                effectTilesCached = null;
            }
            Notify_ChangedState(true);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            Notify_ChangedState(false);
            base.PostDeSpawn(map, mode);
        }

        public void Notify_ChangedState(bool newState)
        {
            if (isWasActive != newState)
            {
                isWasActive = newState;
                compOdysseyMapComponent.vacResistAOEGrid.UpdateGrid(effectTiles, newState);
            }
        }
    }
}