using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Reflection;
using RimWorld;
using Verse.Sound;
using UnityEngine;
using Verse.AI;

namespace OdysseyCompact
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        //private static readonly Type patchType;

        //static HarmonyPatches()
        //{
        //    patchType = typeof(HarmonyPatches);
        //    Harmony val = new Harmony("rimworld.mrhydralisk.PSIPatch");
        //    val.Patch((MethodBase)AccessTools.Method(typeof(WorkGiver_Warden_SuppressSlave), "JobOnThing", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(patchType, "WGWSS_JobOnThing_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
        //    val.Patch((MethodBase)AccessTools.Method(typeof(PrisonBreakUtility), "CanParticipateInPrisonBreak", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(patchType, "PBU_CanParticipateInPrisonBreak_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
        //    val.Patch((MethodBase)AccessTools.Method(typeof(SlaveRebellionUtility), "CanParticipateInSlaveRebellion", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(patchType, "SRU_CanParticipateInSlaveRebellion_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
        //}

        //public static void WGWSS_JobOnThing_Postfix(Pawn pawn, Thing t, WorkGiver_Warden_SuppressSlave __instance, ref Job __result)
        //{
        //    if (__result != null)
        //    {
        //        Pawn pawn2 = t as Pawn;
        //        if (pawn2?.health?.hediffSet?.HasHediff(HediffDefOfLocal.PSISuppressor) ?? false)
        //        {
        //            __result.Clear();
        //            __result = null;
        //        }
        //    }
        //}

        //public static void PBU_CanParticipateInPrisonBreak_Postfix(Pawn pawn, ref bool __result)
        //{
        //    if (__result == true)
        //    {
        //        if (pawn?.health?.hediffSet?.HasHediff(HediffDefOfLocal.PSISubduer) ?? false)
        //        {
        //            __result = false;
        //        }
        //    }
        //}

        //public static void SRU_CanParticipateInSlaveRebellion_Postfix(Pawn pawn, ref bool __result)
        //{
        //    if (__result == true)
        //    {
        //        if (pawn?.health?.hediffSet?.HasHediff(HediffDefOfLocal.PSISubduer) ?? false)
        //        {
        //            __result = false;
        //        }
        //    }
        //}
    }
}