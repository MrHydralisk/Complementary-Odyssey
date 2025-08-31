using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_VacResistAOEProjector : CompProperties
    {
        public float effectRadius = 5.5f;

        public CompProperties_VacResistAOEProjector()
        {
            compClass = typeof(CompVacResistAOEProjector);
        }
    }
}