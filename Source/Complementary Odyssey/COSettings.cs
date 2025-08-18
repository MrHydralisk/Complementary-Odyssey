using Verse;

namespace ComplementaryOdyssey
{
    public class COSettings : ModSettings
    {
        public bool VacRoofPatches = true;
        public int VacRoofPoweredAfterLanding = 250;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref VacRoofPatches, "VacRoofPatches", defaultValue: true);
            Scribe_Values.Look(ref VacRoofPoweredAfterLanding, "VacRoofPoweredAfterLanding", defaultValue: 250);
        }
    }
}

