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
    }
}