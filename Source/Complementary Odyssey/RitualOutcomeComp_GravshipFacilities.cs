using System.Linq;
using Verse;

namespace ComplementaryOdyssey
{
    public class RitualOutcomeComp_GravshipFacilities : RimWorld.RitualOutcomeComp_GravshipFacilities
    {
        protected override string LabelForDesc => facilityQualityOffsets?.Keys.FirstOrDefault()?.label ?? label;
    }
}