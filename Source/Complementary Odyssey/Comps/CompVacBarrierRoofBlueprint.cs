using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompVacBarrierRoofBlueprint : ThingComp
    {
        private CompProperties_VacBarrierRoofBlueprint Props => (CompProperties_VacBarrierRoofBlueprint)props;

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Destroyed)
            {
                parent.Map.roofGrid.SetRoof(parent.Position, Props.roofDef);
                MoteMaker.PlaceTempRoof(parent.Position, parent.Map);
                parent.Destroy();
            }
        }
    }
}