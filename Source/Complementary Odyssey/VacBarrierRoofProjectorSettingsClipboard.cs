using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ComplementaryOdyssey
{
    public static class VacBarrierRoofProjectorSettingsClipboard
    {
        private static IntVec2 barrierSize;
        private static IntVec2 barrierOffset;

        private static bool copied = false;

        public static bool HasCopiedSettings => copied;

        public static void Copy(CompVacBarrierRoofProjector vbrp)
        {
            barrierSize = vbrp.barrierSize;
            barrierOffset = vbrp.barrierOffset;
            copied = true;
            Messages.Message("StorageSettingsCopiedToClipboard".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
        }

        public static void PasteInto(CompVacBarrierRoofProjector vbrp)
        {
            vbrp.barrierSize = barrierSize;
            vbrp.barrierOffset = barrierOffset;
            vbrp.UpdateBarrierTiles();
            Messages.Message("StorageSettingsPastedFromClipboard".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
        }

        public static IEnumerable<Gizmo> CopyPasteGizmosFor(CompVacBarrierRoofProjector vbrp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings");
            command_Action.defaultLabel = "CommandCopyZoneSettingsLabel".Translate();
            command_Action.defaultDesc = "CommandCopyZoneSettingsDesc".Translate();
            command_Action.action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Copy(vbrp);
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
                PasteInto(vbrp);
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