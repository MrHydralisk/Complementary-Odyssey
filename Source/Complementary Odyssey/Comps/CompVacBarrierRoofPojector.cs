using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompVacBarrierRoofPojector : CompPowerTrader
    {
        public new CompProperties_VacBarrierRoofPojector Props => props as CompProperties_VacBarrierRoofPojector;

        public MapComponent_CompOdyssey compOdysseyMapComponent => compOdysseyMapComponentCached ?? (compOdysseyMapComponentCached = parent.MapHeld.GetComponent<MapComponent_CompOdyssey>() ?? null);
        private MapComponent_CompOdyssey compOdysseyMapComponentCached;

        public IntVec2 barrierSize;
        public IntVec2 barrierOffset;

        public override void PostPostMake()
        {
            base.PostPostMake();
            barrierSize = Props.initialBarrierSize;
            barrierOffset = Props.initialBarrierOffset;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                compOdysseyMapComponentCached = null;
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
        }

        public List<IntVec3> barrierTiles()
        {
            return new CellRect(barrierOffset.x - barrierSize.x / 2, barrierOffset.z - barrierSize.z / 2, barrierSize.x, barrierSize.z).Cells.ToList();
        }

        //public bool ShouldShowSurfaceResourceOverlay()
        //{
        //    if (powerComp != null)
        //    {
        //        return powerComp.PowerOn;
        //    }
        //    return false;
        //}

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
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    floatMenuOptions.Add(new FloatMenuOption("+x", delegate
                    {
                        barrierOffset.x = Mathf.Min(barrierOffset.x + 1, Props.maxBarrierOffset.x);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("-x", delegate
                    {
                        barrierOffset.x = Mathf.Max(barrierOffset.x - 1, Props.maxBarrierOffset.y);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("+z", delegate
                    {
                        barrierOffset.z = Mathf.Min(barrierOffset.z + 1, Props.maxBarrierOffset.z);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("-z", delegate
                    {
                        barrierOffset.z = Mathf.Max(barrierOffset.z - 1, 1);
                    }));
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Offset.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Offset.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport"),
                Order = 30,
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    floatMenuOptions.Add(new FloatMenuOption("+x", delegate
                    {
                        barrierSize.x = Mathf.Min(barrierSize.x + 1, Props.maxBarrierSize.x);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("-x", delegate
                    {
                        barrierSize.x = Mathf.Max(barrierSize.x - 1, 1);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("+z", delegate
                    {
                        barrierSize.z = Mathf.Min(barrierSize.z + 1, Props.maxBarrierSize.z);
                    }));
                    floatMenuOptions.Add(new FloatMenuOption("-z", delegate
                    {
                        barrierSize.z = Mathf.Max(barrierSize.z - 1, 1);
                    }));
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Size.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Size.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport"),
                Order = 30,
            };
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        scannedTiles = 0;
            //    },
            //    defaultLabel = "ComplementaryOdyssey.TerrainScanner.Gizmo.Reset.Label".Translate(),
            //    defaultDesc = "ComplementaryOdyssey.TerrainScanner.Gizmo.Reset.Desc".Translate(),
            //    icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport"),
            //    Order = 30,
            //};
            //if (DebugSettings.ShowDevGizmos)
            //{
            //    yield return new Command_Action
            //    {
            //        action = delegate
            //        {
            //            Scan(parent.Map.cellIndices.NumGridCells, 60000);
            //        },
            //        defaultLabel = "Dev: Scan all",
            //        defaultDesc = "Scan whole map"
            //    };
            //}
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            //if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
            //{
            //    inspectStrings.Add("UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent());
            //}
            //if (scannedTiles >= parent.MapHeld.cellIndices.NumGridCells)
            //{
            //    inspectStrings.Add("ComplementaryOdyssey.TerrainScanner.CanUse.ScannedFully".Translate());
            //}
            //else
            //{
            //    inspectStrings.Add("ComplementaryOdyssey.TerrainScanner.InspectString.Progress".Translate(((float)scannedTiles / parent.MapHeld.cellIndices.NumGridCells).ToStringPercent()));
            //}
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref barrierSize, "barrierSize");
            Scribe_Values.Look(ref barrierOffset, "barrierOffset");
        }
    }
}