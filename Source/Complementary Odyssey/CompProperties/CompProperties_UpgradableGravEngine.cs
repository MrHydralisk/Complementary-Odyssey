using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_UpgradableGravEngine : CompProperties
    {
        public ResearchProjectDef unlockedWithResearchProjectDef;

        public CompProperties_UpgradableGravEngine()
        {
            compClass = typeof(CompUpgradableGravEngine);
        }
    }
}