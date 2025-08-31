using Verse;

namespace ComplementaryOdyssey
{
    public class COSettings : ModSettings
    {
        public bool VacRoofPatches = true;
        public int VacRoofPoweredAfterLanding = 250;
        public float VacResistAOEVacOverride = 0.4f;
        public float VacResistAOEGrowthRateFactorTemperature = 0.8f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref VacRoofPatches, "VacRoofPatches", defaultValue: true);
            Scribe_Values.Look(ref VacRoofPoweredAfterLanding, "VacRoofPoweredAfterLanding", defaultValue: 250);
            Scribe_Values.Look(ref VacResistAOEVacOverride, "VacResistAOEVacOverride", defaultValue: 0.4f);
            Scribe_Values.Look(ref VacResistAOEGrowthRateFactorTemperature, "VacResistAOEGrowthRateFactorTemperature", defaultValue: 0.8f);
        }
    }
}

