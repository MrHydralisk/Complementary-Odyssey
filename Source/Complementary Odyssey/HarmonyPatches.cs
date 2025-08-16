using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
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

            val.Patch(AccessTools.Method(typeof(ShipLandingArea), "RecalculateBlockingThing"), transpiler: new HarmonyMethod(patchType, "SLA_RecalculateBlockingThing_Transpiler"));
            val.Patch(AccessTools.Property(typeof(CompLaunchable), "AnyInGroupIsUnderRoof").GetGetMethod(true), transpiler: new HarmonyMethod(patchType, "SLA_RecalculateBlockingThing_Transpiler"));
            val.Patch(AccessTools.Method(typeof(RoofGrid), "GetCellExtraColor"), postfix: new HarmonyMethod(patchType, "RG_GetCellExtraColor_Postfix"));
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



        public static IEnumerable<CodeInstruction> SLA_RecalculateBlockingThing_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && (codes[i].operand?.ToString().Contains("Roofed") ?? false))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "Roofed"));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        public static bool Roofed(IntVec3 c, Map map)
        {
            Log.Message($"{c} {c.Roofed(map)}");
            if (c.Roofed(map))
            {
                RoofDef roofDef = c.GetRoof(map);
                if (roofDef.IsVacRoof(out _))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static void RG_GetCellExtraColor_Postfix(ref Color __result, RoofGrid __instance, int index)
        {
            if (__result == Color.white)
            {
                RoofDef[] roofGrid = AccessTools.Field(typeof(RoofGrid), "roofGrid").GetValue(__instance) as RoofDef[];
                if (roofGrid[index].IsVacRoof(out ComplementaryOdysseyDefModExtension defModExtension))
                {
                    __result = defModExtension.color;
                }
            }
        }
    }
}
