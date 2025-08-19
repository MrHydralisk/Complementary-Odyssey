using RimWorld;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompVacRoofBlueprint : ThingComp
    {
        private CompProperties_VacRoofBlueprint Props => (CompProperties_VacRoofBlueprint)props;

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