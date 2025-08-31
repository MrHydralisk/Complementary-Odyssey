using RimWorld;
using Verse;

namespace ComplementaryOdyssey
{
    public class Designator_BuildCompressedHay : Designator_Build
    {
        public Designator_BuildCompressedHay() : base(DefOfLocal.CO_CompressedHay)
        {
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
                GenConstruct.PlaceBlueprintForBuild(entDef, c, Map, placingRot, Faction.OfPlayer, null);
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}