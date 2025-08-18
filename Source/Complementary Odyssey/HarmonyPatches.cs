using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Noise;

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

            AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing").Invoke(null, new object[] { DefOfLocal.CO_VacBarrierRoofFraming, false, null, true });
            AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing").Invoke(null, new object[] { DefOfLocal.CO_VacBarrierRoofFraming, true });

            val.Patch(AccessTools.Method(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs"), transpiler: new HarmonyMethod(patchType, "MI_AfterMainTabs_Transpiler"));
            val.Patch(AccessTools.Method(typeof(Mineable), "DestroyMined"), prefix: new HarmonyMethod(patchType, "M_DestroyMined_Prefix"));

            if (COMod.Settings.VacRoofPatches)
            {
                val.Patch(AccessTools.Method(typeof(ShipLandingArea), "RecalculateBlockingThing"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Property(typeof(CompLaunchable), "AnyInGroupIsUnderRoof").GetGetMethod(true), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(RoofGrid), "GetCellExtraColor"), postfix: new HarmonyMethod(patchType, "RG_GetCellExtraColor_Postfix"));
                val.Patch(AccessTools.Method(typeof(RoofGrid), "SetRoof"), prefix: new HarmonyMethod(patchType, "RG_SetRoof_Prefix"));
                val.Patch(AccessTools.Method(typeof(Skyfaller).GetNestedTypes(AccessTools.all).First((Type t) => t.Name.Contains("c__DisplayClass57_0")), "<HitRoof>b__0"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(DropCellFinder), "CanPhysicallyDropInto"), transpiler: new HarmonyMethod(patchType, "ReplaceGetRoof_Transpiler"));
                val.Patch(AccessTools.Method(typeof(RoyalTitlePermitWorker_CallShuttle), "GetReportFromCell"), transpiler: new HarmonyMethod(patchType, "ReplaceGetRoof_Transpiler"));

                val.Patch(AccessTools.Method(typeof(Building_TurretGun), "TryStartShootSomething"), transpiler: new HarmonyMethod(patchType, "BTG_TryStartShootSomething_Transpiler"));
                val.Patch(AccessTools.Method(typeof(Building_TurretGun), "GetInspectString"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(Building_TurretGun).GetNestedTypes(AccessTools.all).First((Type t) => t.Name.Contains("<GetGizmos>d__71")), "MoveNext"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(PlaceWorker_NotUnderRoof), "AllowsPlacing"), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofed_Transpiler"));

                val.Patch(AccessTools.Property(typeof(CompPowerPlantSolar), "RoofedPowerOutputFactor").GetGetMethod(true), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(GlowGrid), "GroundGlowAt"), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(ThoughtWorker_Aurora), "CurrentStateInternal"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(SectionLayer_LightingOverlay), "GenerateLightingOverlay"), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(SectionLayer_LightingOverlay), "GenerateLightingOverlay"), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofAt_Transpiler"));
                val.Patch(AccessTools.Method(typeof(RoofUtility), "IsAnyCellUnderRoof"), transpiler: new HarmonyMethod(patchType, "ReplaceGridRoofed_Transpiler"));

                val.Patch(AccessTools.Method(typeof(InfestationCellFinder), "GetScoreAt"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(Need_Outdoors), "NeedInterval"), transpiler: new HarmonyMethod(patchType, "ReplaceGetRoof_Transpiler"));
                
                val.Patch(AccessTools.Method(typeof(District), "OpenRoofCountStopAt"), transpiler: new HarmonyMethod(patchType, "ReplacePoweredGridRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(District), "ExposedVacuumCount"), transpiler: new HarmonyMethod(patchType, "ReplacePoweredGridRoofed_Transpiler"));

                val.Patch(AccessTools.Method(typeof(JobDriver_Skygaze), "<MakeNewToils>b__1_2"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(JobDriver_Skydreaming), "<MakeNewToils>b__1_2"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
                val.Patch(AccessTools.Method(typeof(ThoughtWorker_IsOutdoorsForUndergrounder), "CurrentStateInternal"), transpiler: new HarmonyMethod(patchType, "ReplaceRoofed_Transpiler"));
            }
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
            MapComponent_CompOdyssey mapComponent_CompOdyssey = MapComponent_CompOdyssey.CachedInstance(Find.CurrentMap);
            if (mapComponent_CompOdyssey != null)
            {
                mapComponent_CompOdyssey.surfaceResourceGrid.GridOnGUI();
                mapComponent_CompOdyssey.vacRoofGrid.GridOnGUI();
            }
        }

        public static bool M_DestroyMined_Prefix(Mineable __instance)
        {
            MapComponent_CompOdyssey.CachedInstance(__instance.Map).surfaceResourceGrid.SetDirty();
            return true;
        }



        public static IEnumerable<CodeInstruction> ReplaceRoofed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && (codes[i].operand?.ToString().Contains("Roofed") ?? false))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "Roofed"));
                }
            }
            return codes.AsEnumerable();
        }

        public static void RG_GetCellExtraColor_Postfix(ref Color __result, RoofGrid __instance, int index)
        {
            if (__result == Color.white)
            {
                RoofDef roofDef = __instance.RoofAt(index);
                if (roofDef.IsVacRoof(out ComplementaryOdysseyDefModExtension defModExtension))
                {
                    __result = defModExtension.color;
                }
            }
        }

        public static bool RG_SetRoof_Prefix(RoofGrid __instance, IntVec3 c, RoofDef def, Map ___map)
        {
            RoofDef roofDef = __instance.RoofAt(c);
            if (roofDef.IsVacRoof(out _) && def != roofDef && roofDef.collapseLeavingThingDef != null)
            {
                GenSpawn.Spawn(roofDef.collapseLeavingThingDef, c, ___map);
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> ReplaceGetRoof_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && (codes[i].operand?.ToString().Contains("GetRoof") ?? false))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "GetRoof"));
                }
            }
            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> BTG_TryStartShootSomething_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && (codes[i].operand?.ToString().Contains("Roofed") ?? false))
                {
                    CodeInstruction codeInstruction = codes[i - 4];
                    codes[i - 4] = codes[i - 1];
                    codes[i - 1] = codeInstruction;
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "Roofed"));
                    codes.RemoveAt(i - 3);
                }
            }
            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> ReplaceGridRoofed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand?.ToString().Contains("Roofed(Verse.IntVec3)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "GridRoofed", new Type[] { typeof(RoofGrid), typeof(IntVec3) }));
                    }
                    else if (codes[i].operand?.ToString().Contains("Roofed(Int32)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "GridRoofed", new Type[] { typeof(RoofGrid), typeof(int) }));
                    }
                }
            }
            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> ReplaceGridRoofAt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand?.ToString().Contains("RoofAt(Verse.IntVec3)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "GridRoofAt", new Type[] { typeof(RoofGrid), typeof(IntVec3) }));
                    }
                    else if (codes[i].operand?.ToString().Contains("RoofAt(Int32)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "GridRoofAt", new Type[] { typeof(RoofGrid), typeof(int) }));
                    }
                }
            }
            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> ReplacePoweredGridRoofed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand?.ToString().Contains("Roofed(Verse.IntVec3)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "PoweredGridRoofed", new Type[] { typeof(RoofGrid), typeof(IntVec3) }));
                    }
                    else if (codes[i].operand?.ToString().Contains("Roofed(Int32)") ?? false)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComplementaryOdysseyUtility), "PoweredGridRoofed", new Type[] { typeof(RoofGrid), typeof(int) }));
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}
