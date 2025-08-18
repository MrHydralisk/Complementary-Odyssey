using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ComplementaryOdyssey
{
    public class Designator_BuildVacBarrierRoof : Designator_Build
    {
        private readonly RoofDef roofDef;

        public override BuildableDef PlacingDef => entDef;

        public Designator_BuildVacBarrierRoof() : base(DefOfLocal.CO_VacBarrierRoofFraming)
        {
            this.roofDef = DefOfLocal.CO_VacBarrierRoof;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (loc.GetFirstThing(Map, entDef.blueprintDef) != null)
            {
                return false;
            }
            RoofDef roofDef = Map.roofGrid.RoofAt(loc);
            if (roofDef == null)
            {
                return true;
            }
            return !roofDef.isThickRoof && roofDef != this.roofDef;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (DebugSettings.godMode)
            {
                Thing thing = ThingMaker.MakeThing((ThingDef)entDef);
                thing.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(thing, c, Map, placingRot);
            }
            else
            {
                Map.areaManager.NoRoof[c] = false;
                GenConstruct.PlaceBlueprintForBuild(entDef, c, Map, placingRot, Faction.OfPlayer, null);
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}