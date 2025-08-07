using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_ShortRangeMineralScanner : CompProperties_Scanner
    {
        public CompProperties_ShortRangeMineralScanner()
        {
            compClass = typeof(CompShortRangeMineralScanner);
        }
    }
}