using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class VacPlant : Plant
    {
        protected bool InVacuum => PositionHeld.GetVacuumOld(MapHeld) > 0f;
        public bool isAutoHarvest = true;

        protected override bool Resting => false;
        public override float GrowthRate => base.GrowthRate * (InVacuum ? 4f : 1f);

        public override void TickLong()
        {
            base.TickLong();
            if (isAutoHarvest && !Destroyed && Spawned && LifeStage == PlantLifeStage.Mature && !Map.designationManager.HasMapDesignationOn(this))
            {
                Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.HarvestPlant));
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            Command_Toggle command_Toggle = new Command_Toggle();
            command_Toggle.defaultLabel = "ComplementaryOdyssey.VacPlant.Gizmo.isAutoHarvest.Label".Translate();
            command_Toggle.defaultDesc = "ComplementaryOdyssey.VacPlant.Gizmo.isAutoHarvest.Desc".Translate(Label, def.plant.harvestedThingDef.label);
            command_Toggle.isActive = () => isAutoHarvest;
            command_Toggle.toggleAction = delegate
            {
                isAutoHarvest = !isAutoHarvest;
            };
            command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
            command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest");
            command_Toggle.hotKey = KeyBindingDefOf.Command_ItemForbid;
            command_Toggle.Order = 30;
            yield return command_Toggle;
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            string baseInspectString = base.GetInspectString();
            if (!baseInspectString.NullOrEmpty())
            {
                inspectStrings.Add(baseInspectString);
            }
            if (InVacuum)
            {
                inspectStrings.Add($"{"AlertVacuumExposure".Translate()}: x400%");
            }
            return String.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isAutoHarvest, "isAutoHarvest", true);
        }
    }
}