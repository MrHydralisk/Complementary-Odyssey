using RimWorld;
using Verse;

namespace ComplementaryOdyssey
{
    public class Designator_BuildVacRoof : Designator_Build
    {
        private readonly RoofDef roofDef;

        public override BuildableDef PlacingDef => entDef;

        public Designator_BuildVacRoof() : base(DefOfLocal.CO_VacRoofFraming)
        {
            this.roofDef = DefOfLocal.CO_VacRoof;
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