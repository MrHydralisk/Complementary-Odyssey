using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace ComplementaryOdyssey
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("rimworld.mrhydralisk.ComplementaryOdyssey");
            val.Patch(AccessTools.Method(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs"), transpiler: new HarmonyMethod(patchType, "MI_AfterMainTabs_Transpiler"));
            val.Patch(AccessTools.Method(typeof(Mineable), "DestroyMined"), prefix: new HarmonyMethod(patchType, "M_DestroyMined_Prefix"));
        }

        public static IEnumerable<CodeInstruction> MI_AfterMainTabs_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && (codes[i].operand?.ToString().Contains("DeepResourcesOnGUI") ?? false))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "SurfaceResourcesOnGUI")));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        public static void SurfaceResourcesOnGUI()
        {
            MapComponent_CompOdyssey.CachedInstance(Find.CurrentMap)?.surfaceResourceGrid.SurfaceResourcesOnGUI();
        }

        public static bool M_DestroyMined_Prefix(Mineable __instance)
        {
            MapComponent_CompOdyssey.CachedInstance(__instance.Map).surfaceResourceGrid.SetDirty();
            return true;
        }
    }
}
