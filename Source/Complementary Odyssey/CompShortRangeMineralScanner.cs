using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompShortRangeMineralScanner : CompScanner
    {
        public new CompProperties_ShortRangeMineralScanner Props => props as CompProperties_ShortRangeMineralScanner;

        public MapComponent_CompOdyssey compOdysseyMapComponent => compOdysseyMapComponentCached ?? (compOdysseyMapComponentCached = parent.Map.GetComponent<MapComponent_CompOdyssey>() ?? null);
        private MapComponent_CompOdyssey compOdysseyMapComponentCached;

        public int scannedTiles;

        public override AcceptanceReport CanUseNow
        {
            get
            {
                if (scannedTiles >= parent.Map.cellIndices.NumGridCells)
                {
                    return "ComplementaryOdyssey.ShortRangeMineralScanner.CanUse.ScannedFully".Translate();
                }
                return base.CanUseNow;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                scannedTiles = 0;
            }
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (ShouldShowSurfaceResourceOverlay())
            {
                compOdysseyMapComponent.surfaceResourceGrid.MarkForDraw();
            }
        }

        public bool ShouldShowSurfaceResourceOverlay()
        {
            if (powerComp != null)
            {
                return powerComp.PowerOn;
            }
            return false;
        }

        protected override bool TickDoesFind(float scanSpeed)
        {
            if (parent.IsHashIntervalTick(Props.ticksPerScan))
            {
                return true;
            }
            return false;
        }

        protected override void DoFind(Pawn worker)
        {
            Map map = parent.Map;
            int canScanAmount = Mathf.Max(1, Mathf.RoundToInt(Props.tilesPerScan * worker.GetStatValue(Props.scanSpeedStat)));
            int scanned = 0;
            int iterations = 0;
            while (scannedTiles < parent.Map.cellIndices.NumGridCells && iterations < canScanAmount && iterations < 1000)
            {
                IntVec3 cell = map.cellIndices.IndexToCell(scannedTiles);
                scannedTiles++;
                iterations++;
                Mineable mineable = cell.GetFirstMineable(map);
                if (mineable != null)
                {
                    compOdysseyMapComponent.surfaceResourceGrid.SetAt(cell);
                    scanned++;
                    if (!(mineable.def.building.mineableThing?.thingCategories?.Contains(ThingCategoryDefOf.StoneChunks) ?? true))
                    {
                        bool isNewVein = true;
                        foreach (IntVec3 adj in GenAdjFast.AdjacentCells8Way(cell))
                        {
                            if (adj.InBounds(map) && map.cellIndices.CellToIndex(adj) < scannedTiles && adj.GetFirstMineable(map)?.def == mineable.def)
                            {
                                isNewVein = false;
                                break;
                            }
                        }
                        if (isNewVein)
                        {
                            TargetInfo targetInfo = new TargetInfo(cell, map);
                            Messages.Message($"ComplementaryOdyssey.ShortRangeMineralScanner.Message.FoundOre".Translate(mineable.def.label, parent.LabelCap).RawText, targetInfo, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
        }

        private bool CanScatterAt(IntVec3 pos, Map map)
        {
            int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
            TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);
            if ((terrainDef != null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable) || !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
            {
                return false;
            }
            return !compOdysseyMapComponent.surfaceResourceGrid.GetCellBool(index);
        }

        protected ThingDef ChooseLumpThingDef()
        {
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
            {
                inspectStrings.Add("UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent());
            }
            if (scannedTiles >= parent.Map.cellIndices.NumGridCells)
            {
                inspectStrings.Add("ComplementaryOdyssey.ShortRangeMineralScanner.CanUse.ScannedFully".Translate());
            }
            else
            {
                inspectStrings.Add("ComplementaryOdyssey.ShortRangeMineralScanner.InspectString.Progress".Translate(((float)scannedTiles / parent.Map.cellIndices.NumGridCells).ToStringPercent()));
            }
            return String.Join("\n", inspectStrings);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref scannedTiles, "scannedTiles", 0);
        }
    }
}