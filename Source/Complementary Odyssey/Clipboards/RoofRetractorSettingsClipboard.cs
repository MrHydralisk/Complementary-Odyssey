using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ComplementaryOdyssey
{
    public static class RoofRetractorSettingsClipboard
    {
        private static IntVec2 borders;

        private static bool copied = false;

        public static bool HasCopiedSettings => copied;

        public static void Copy(CompRoofRetractor crr)
        {
            borders = crr.borders;
            copied = true;
            Messages.Message("StorageSettingsCopiedToClipboard".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
        }

        public static void PasteInto(CompRoofRetractor crr)
        {
            crr.borders = borders;
            Messages.Message("StorageSettingsPastedFromClipboard".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
        }

        public static IEnumerable<Gizmo> CopyPasteGizmosFor(CompRoofRetractor crr)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings");
            command_Action.defaultLabel = "CommandCopyZoneSettingsLabel".Translate();
            command_Action.defaultDesc = "CommandCopyZoneSettingsDesc".Translate();
            command_Action.action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Copy(crr);
            };
            command_Action.hotKey = KeyBindingDefOf.Misc4;
            yield return command_Action;
            Command_Action command_Action2 = new Command_Action();
            command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings");
            command_Action2.defaultLabel = "CommandPasteZoneSettingsLabel".Translate();
            command_Action2.defaultDesc = "CommandPasteZoneSettingsDesc".Translate();
            command_Action2.action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                PasteInto(crr);
            };
            command_Action2.hotKey = KeyBindingDefOf.Misc5;
            if (!HasCopiedSettings)
            {
                command_Action2.Disable();
            }
            yield return command_Action2;
        }
    }
}