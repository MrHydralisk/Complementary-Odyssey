using RimWorld;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class COMod : Mod
    {
        public static COSettings Settings { get; private set; }

        private string inputBufferGravFieldExtenderOffsetMult;

        public COMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<COSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard options = new Listing_Standard();
            options.Begin(inRect);
            options.CheckboxLabeled("ComplementaryOdyssey.Settings.VacRoofPatches".Translate().RawText, ref Settings.VacRoofPatches);
            options.Label("ComplementaryOdyssey.Settings.VacRoofPoweredAfterLanding".Translate(Settings.VacRoofPoweredAfterLanding.ToStringTicksToPeriod()));
            Settings.VacRoofPoweredAfterLanding = (int)options.Slider(Settings.VacRoofPoweredAfterLanding, 0, 7500);
            options.GapLine();
            options.CheckboxLabeled("ComplementaryOdyssey.Settings.VacResistAOEPatches".Translate().RawText, ref Settings.VacResistAOEPatches);
            options.Label("ComplementaryOdyssey.Settings.VacflowerChance".Translate(Settings.VacflowerChance.ToStringPercent()));
            Settings.VacflowerChance = Mathf.Round(options.Slider(Settings.VacflowerChance, 0f, 1f) * 100f) / 100f;
            options.Label("ComplementaryOdyssey.Settings.VacResistAOEVacOverride".Translate(Settings.VacResistAOEVacOverride.ToStringPercent()));
            Settings.VacResistAOEVacOverride = Mathf.Round(options.Slider(Settings.VacResistAOEVacOverride, 0.01f, 0.49f) * 100f) / 100f;
            options.Label("ComplementaryOdyssey.Settings.VacResistAOEGrowthRateFactorTemperature".Translate(Settings.VacResistAOEGrowthRateFactorTemperature.ToStringPercent()));
            Settings.VacResistAOEGrowthRateFactorTemperature = Mathf.Round(options.Slider(Settings.VacResistAOEGrowthRateFactorTemperature, 0.01f, 1f) * 100f) / 100f;
            options.GapLine();
            float SubstructureSupportPerOne = ThingDefOf.GravFieldExtender?.GetCompProperties<CompProperties_GravshipFacility>()?.statOffsets?.GetStatOffsetFromList(StatDefOf.SubstructureSupport) ?? 250;
            options.Label("ComplementaryOdyssey.Settings.GravFieldExtenderOffsetMult".Translate(ThingDefOf.GravFieldExtender.label, ThingDefOf.GravEngine.label, Settings.GravFieldExtenderOffsetMult.ToStringPercent(), Mathf.Round(SubstructureSupportPerOne * Settings.GravFieldExtenderOffsetMult * 100f) / 100f, StatDefOf.SubstructureSupport.label, SubstructureSupportPerOne));
            Settings.GravFieldExtenderOffsetMult = Mathf.Round(options.Slider(Settings.GravFieldExtenderOffsetMult, 0.01f, 1f) * 100f) / 100f;
            options.Label("ComplementaryOdyssey.Settings.GravFieldExtenderMaxAmount".Translate(ThingDefOf.GravFieldExtender.label, ThingDefOf.GravEngine.label));
            options.TextFieldNumeric(ref Settings.GravFieldExtenderMaxAmount, ref inputBufferGravFieldExtenderOffsetMult, 0, 100000);
            options.End();            
        }

        public override string SettingsCategory()
        {
            return "ComplementaryOdyssey.Settings.Title".Translate().RawText;
        }
    }
}
