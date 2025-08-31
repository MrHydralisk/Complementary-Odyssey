using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_VacResistAOEProjector : CompProperties
    {
        public float effectRadius = 5f;

        public CompProperties_VacResistAOEProjector()
        {
            compClass = typeof(CompVacResistAOEProjector);
        }
    }
}