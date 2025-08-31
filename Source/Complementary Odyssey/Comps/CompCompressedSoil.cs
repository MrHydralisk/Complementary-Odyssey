using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompCompressedSoil : ThingComp
    {
        private CompProperties_CompressedSoil Props => (CompProperties_CompressedSoil)props;
        public int ticksTillCompression;

        public override void PostPostMake()
        {
            base.PostPostMake();
            ticksTillCompression = Props.ticksPerCompression;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Destroyed && ticksTillCompression > 0)
            {
                ticksTillCompression--;
                if (ticksTillCompression <= 0)
                {
                    parent.Map.terrainGrid.SetTerrain(parent.Position, Props.terrainDef);
                    parent.Destroy();
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ticksTillCompression = 1;
                    },
                    defaultLabel = "Dev: Finish decomposition",
                    defaultDesc = "Will set decomposition timer to 1 tick."
                };
            }
        }

        public override string CompInspectStringExtra()
        {
            return "WorldObjectTimeout".Translate(ticksTillCompression.ToStringTicksToPeriodVerbose());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksTillCompression, "ticksTillCompression");
        }
    }
}