using RimWorld;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class COMod : Mod
    {
        public static COSettings Settings { get; private set; }

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
            options.End();
        }

        public override string SettingsCategory()
        {
            return "ComplementaryOdyssey.Settings.Title".Translate().RawText;
        }
    }
}
