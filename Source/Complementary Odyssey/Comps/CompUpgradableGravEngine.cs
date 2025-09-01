using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace ComplementaryOdyssey
{
    public class CompUpgradableGravEngine : ThingComp
    {
        public int GravFieldExtenderInstalled;
        public float SubstructureSupportPerOne;
        public CompProperties_UpgradableGravEngine Props => props as CompProperties_UpgradableGravEngine;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SubstructureSupportPerOne = ThingDefOf.GravFieldExtender.GetCompProperties<CompProperties_GravshipFacility>()?.statOffsets?.GetStatOffsetFromList(StatDefOf.SubstructureSupport) ?? 250;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            for ( int i = 0; i < GravFieldExtenderInstalled; i++)
            {
                GenSpawn.Spawn(ThingDefOf.GravFieldExtender, parent.Position, previousMap).Destroy(DestroyMode.Deconstruct);
            }
            base.PostDestroy(mode, previousMap);
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDefOf.SubstructureSupport)
            {
                return SubstructureSupportPerOne * COMod.Settings.GravFieldExtenderOffsetMult * GravFieldExtenderInstalled;
            }
            return base.GetStatOffset(stat);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer && Props.unlockedWithResearchProjectDef.IsFinished)
            {
                List<Thing> outThings = new List<Thing>();
                GenAdjFast.AdjacentThings8Way(parent, outThings);
                Thing thing = outThings.FirstOrDefault((Thing t) => t.def == ThingDefOf.GravFieldExtender);
                Command_Action GravFieldExtenderInstallCommand = new Command_Action
                {
                    action = delegate
                    {
                        GravFieldExtenderInstalled++;
                        thing.Destroy();
                    },
                    defaultLabel = "ComplementaryOdyssey.UpgradableGravEngine.Gizmo.GravFieldExtenderInstall.Label".Translate(ThingDefOf.GravFieldExtender.label),
                    defaultDesc = "ComplementaryOdyssey.UpgradableGravEngine.Gizmo.GravFieldExtenderInstall.Desc".Translate(ThingDefOf.GravFieldExtender.label, parent.Label, SubstructureSupportPerOne * COMod.Settings.GravFieldExtenderOffsetMult, StatDefOf.SubstructureSupport.label),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/CO_InstallGravFieldExtender"),
                    Order = 30
                    
                };
                if (thing == null)
                {
                    GravFieldExtenderInstallCommand.Disabled = true;
                    GravFieldExtenderInstallCommand.disabledReason = "ComplementaryOdyssey.UpgradableGravEngine.Gizmo.GravFieldExtenderInstall.Reason.Missing".Translate(ThingDefOf.GravFieldExtender.label, parent.Label);
                }
                else if (COMod.Settings.GravFieldExtenderMaxAmount > 0 && GravFieldExtenderInstalled >= COMod.Settings.GravFieldExtenderMaxAmount)
                {
                    GravFieldExtenderInstallCommand.Disabled = true;
                    GravFieldExtenderInstallCommand.disabledReason = "ComplementaryOdyssey.UpgradableGravEngine.Gizmo.GravFieldExtenderInstall.Reason.Maximum".Translate(ThingDefOf.GravFieldExtender.label);
                }
                yield return GravFieldExtenderInstallCommand;
            }
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            if (GravFieldExtenderInstalled > 0 && stat == StatDefOf.SubstructureSupport)
            {
                sb.AppendLine();
                sb.AppendLine(whitespace + "ComplementaryOdyssey.UpgradableGravEngine.StatsReport.Group".Translate());
                sb.AppendLine(whitespace + "ComplementaryOdyssey.UpgradableGravEngine.StatsReport.Line".Translate(ThingDefOf.GravFieldExtender.label, SubstructureSupportPerOne * COMod.Settings.GravFieldExtenderOffsetMult, GravFieldExtenderInstalled, SubstructureSupportPerOne * COMod.Settings.GravFieldExtenderOffsetMult * GravFieldExtenderInstalled));
            }
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectString = new List<string>();
            if (GravFieldExtenderInstalled > 0)
            {
                inspectString.Add("ComplementaryOdyssey.UpgradableGravEngine.InspectString.Installed".Translate(ThingDefOf.GravFieldExtender.label, GravFieldExtenderInstalled));
            }
            return string.Join("\n", inspectString);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref GravFieldExtenderInstalled, "GravFieldExtenderInstalled", 0);
        }
    }
}