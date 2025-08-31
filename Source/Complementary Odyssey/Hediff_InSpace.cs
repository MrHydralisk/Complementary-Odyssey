using RimWorld.Planet;
using Verse;

namespace ComplementaryOdyssey
{
    public class Hediff_InSpace : HediffWithComps
    {
        public ComplementaryOdysseyDefModExtension complementaryOdysseyDefModExtension => complementaryOdysseyDefModExtensionCached ?? (complementaryOdysseyDefModExtensionCached = def?.GetModExtension<ComplementaryOdysseyDefModExtension>());
        private ComplementaryOdysseyDefModExtension complementaryOdysseyDefModExtensionCached;

        protected bool InSpace
        {
            get
            {
                if (pawn.SpawnedOrAnyParentSpawned)
                {
                    PlanetTile tile = pawn.MapHeld.Tile;
                    if (tile.Valid)
                    {
                        return tile.LayerDef.isSpace;
                    }
                    return false;
                }
                return false;
            }
        }

        public override string SeverityLabel
        {
            get
            {
                if (Severity <= 0f)
                {
                    return null;
                }
                return Severity.ToStringPercent("F0");
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (InSpace && complementaryOdysseyDefModExtension != null && !complementaryOdysseyDefModExtension.stagesInSpace.NullOrEmpty())
                {
                    return complementaryOdysseyDefModExtension.stagesInSpace[CurStageIndex];
                }
                if (!def.stages.NullOrEmpty())
                {
                    return def.stages[CurStageIndex];
                }
                return null;
            }
        }
    }
}

