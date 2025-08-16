using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public static class ComplementaryOdysseyUtility
    {
        public static bool IsVacRoof(this RoofDef roofDef, out ComplementaryOdysseyDefModExtension defModExtension)
        {
            defModExtension = roofDef.GetModExtension<ComplementaryOdysseyDefModExtension>();
            return defModExtension != null;
        }

        public static bool CanPassRoof(this IntVec3 loc, Map map)
        {
            MapComponent_CompOdyssey compOdysseyMapComponent = MapComponent_CompOdyssey.CachedInstance(map);
            return compOdysseyMapComponent.vacRoofGrid.GetCellBool(loc);
        }



        public static bool Roofed(IntVec3 c, Map map)
        {
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

        public static RoofDef GetRoof(IntVec3 c, Map map)
        {
            RoofDef roofDef = map.roofGrid.RoofAt(c);
            if (roofDef?.IsVacRoof(out _) ?? false)
            {
                return null;
            }
            return roofDef;
        }

        public static bool GridRoofed(RoofGrid roofGrid, IntVec3 c)
        {
            Map map = AccessTools.Field(typeof(RoofGrid), "map").GetValue(roofGrid) as Map;
            return GridRoofed(roofGrid, map.cellIndices.CellToIndex(c));
        }

        public static bool GridRoofed(RoofGrid roofGrid, int index)
        {
            return !(roofGrid.RoofAt(index)?.IsVacRoof(out _) ?? true);
        }

        public static RoofDef GridRoofAt(RoofGrid roofGrid, IntVec3 c)
        {
            Map map = AccessTools.Field(typeof(RoofGrid), "map").GetValue(roofGrid) as Map;
            return GridRoofAt(roofGrid, map.cellIndices.CellToIndex(c));
        }

        public static RoofDef GridRoofAt(RoofGrid roofGrid, int index)
        {
            RoofDef roofDef = roofGrid.RoofAt(index);
            if (roofDef?.IsVacRoof(out _) ?? false)
            {
                return null;
            }
            return roofDef;
        }
    }
}