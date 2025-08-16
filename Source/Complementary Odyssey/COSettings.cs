using Verse;

namespace ComplementaryOdyssey
{
    public class COSettings : ModSettings
    {
        public bool VacRoofPatches = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref VacRoofPatches, "VacRoofPatches", defaultValue: true);
        }
    }
}

