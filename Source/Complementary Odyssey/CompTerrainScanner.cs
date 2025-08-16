using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompTerrainScanner : CompScanner
    {
        public new CompProperties_TerrainScanner Props => props as CompProperties_TerrainScanner;

        public MapComponent_CompOdyssey compOdysseyMapComponent => compOdysseyMapComponentCached ?? (compOdysseyMapComponentCached = parent.MapHeld.GetComponent<MapComponent_CompOdyssey>() ?? null);
        private MapComponent_CompOdyssey compOdysseyMapComponentCached;

        public int scannedTiles;

        public override AcceptanceReport CanUseNow
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (powerComp != null && !powerComp.PowerOn)
                {
                    return false;
                }
                if (forbiddable != null && forbiddable.Forbidden)
                {
                    return false;
                }
                if (scannedTiles >= parent.Map.cellIndices.NumGridCells)
                {
                    return "ComplementaryOdyssey.TerrainScanner.CanUse.ScannedFully".Translate();
                }
                if (parent.Faction != Faction.OfPlayer)
                {
                    return false;
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                scannedTiles = 0;
                compOdysseyMapComponentCached = null;
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
            Scan(Mathf.Max(1, Mathf.RoundToInt(Props.tilesPerScan * worker.GetStatValue(Props.scanSpeedStat))));
        }

        public void Scan(int canScanAmount, int iterationsMax = 1000)
        {
            Map map = parent.Map;
            int scanned = 0;
            int iterations = 0;
            while (scannedTiles < parent.Map.cellIndices.NumGridCells && iterations < canScanAmount && iterations < iterationsMax)
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
                            Messages.Message($"ComplementaryOdyssey.TerrainScanner.Message.FoundOre".Translate(mineable.def.label, parent.LabelCap).RawText, targetInfo, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    scannedTiles = 0;
                },
                defaultLabel = "ComplementaryOdyssey.TerrainScanner.Gizmo.Reset.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.TerrainScanner.Gizmo.Reset.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport"),
                Order = 30,
            };
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Scan(parent.Map.cellIndices.NumGridCells, 60000);
                    },
                    defaultLabel = "Dev: Scan all",
                    defaultDesc = "Scan whole map"
                };
            }
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
            {
                inspectStrings.Add("UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent());
            }
            if (scannedTiles >= parent.MapHeld.cellIndices.NumGridCells)
            {
                inspectStrings.Add("ComplementaryOdyssey.TerrainScanner.CanUse.ScannedFully".Translate());
            }
            else
            {
                inspectStrings.Add("ComplementaryOdyssey.TerrainScanner.InspectString.Progress".Translate(((float)scannedTiles / parent.MapHeld.cellIndices.NumGridCells).ToStringPercent()));
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