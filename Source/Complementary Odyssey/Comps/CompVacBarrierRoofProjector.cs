using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompVacBarrierRoofProjector : CompPowerTrader
    {
        public new CompProperties_VacBarrierRoofProjector Props => props as CompProperties_VacBarrierRoofProjector;

        public MapComponent_CompOdyssey compOdysseyMapComponent => compOdysseyMapComponentCached ?? (compOdysseyMapComponentCached = parent.MapHeld.GetComponent<MapComponent_CompOdyssey>() ?? null);
        private MapComponent_CompOdyssey compOdysseyMapComponentCached;

        public List<IntVec3> barrierTiles
        {
            get
            {
                if (barrierTilesCached.NullOrEmpty())
                {
                    UpdateBarrierTiles();
                }
                return barrierTilesCached;
            }
        }
        public List<IntVec3> barrierTilesCached;

        public IntVec2 barrierSize;
        public IntVec2 barrierOffset;

        public bool isWasPowered;

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
            barrierTilesCached = null;
            isWasPowered = PowerOn;
            Notify_ChangedPowerState(PowerOn);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            Notify_ChangedPowerState(false);
            base.PostDeSpawn(map, mode);
        }

        public List<IntVec3> BarrierTiles()
        {
            return new CellRect(barrierOffset.x, barrierOffset.z, barrierSize.x, barrierSize.z).Cells.ToList();
        }

        public List<IntVec3> BarrierTilesRotated()
        {
            List<IntVec3> tiles = new List<IntVec3>();
            foreach (IntVec3 tile in BarrierTiles())
            {
                tiles.Add(parent.Position + tile.RotatedBy(parent.Rotation));
            }
            return tiles;
        }

        public void UpdateBarrierTiles()
        {
            barrierTilesCached = BarrierTilesRotated();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                UpdatePowerOutput();
                if (isWasPowered != PowerOn)
                {
                    isWasPowered = PowerOn;
                    Notify_ChangedPowerState(PowerOn);
                }
            }
        }

        public void UpdatePowerOutput()
        {
            int num = 1;
            foreach (IntVec3 tile in barrierTiles)
            {
                RoofDef roofDef = parent.Map.roofGrid.RoofAt(tile);
                if (roofDef != null && ComplementaryOdysseyUtility.IsVacRoof(roofDef, out _))
                {
                    num++;
                }
            }
            PowerOutput = Props.PowerConsumption * num;
        }

        public void Notify_ChangedPowerState(bool newState)
        {
            compOdysseyMapComponent.vacRoofGrid.UpdatePowerGrid(barrierTiles, newState);
        }

        public void ChangeBarrier(IntVec2 offset, IntVec2 size)
        {
            if (offset != IntVec2.Invalid)
            {
                barrierOffset = new IntVec2(Mathf.Max(Mathf.Min(barrierOffset.x + offset.x, Props.maxBarrierOffset.x), Props.maxBarrierOffset.y), Mathf.Max(Mathf.Min(barrierOffset.z + offset.z, Props.maxBarrierOffset.z), 1));
            }
            if (size != IntVec2.Invalid)
            {
                barrierSize = new IntVec2(Mathf.Max(Mathf.Min(barrierSize.x + size.x, Props.maxBarrierSize.x), 1), Mathf.Max(Mathf.Min(barrierSize.z + size.z, Props.maxBarrierSize.z), 1));
            }
            UpdateBarrierTiles();
        }

        public bool CanChangeBarrier(IntVec2 offset, IntVec2 size)
        {
            IntVec2 rectOffset;
            IntVec2 rectSize;
            if (offset == IntVec2.Invalid)
            {
                rectOffset = barrierOffset;
            }
            else
            {
                rectOffset = new IntVec2(Mathf.Max(Mathf.Min(barrierOffset.x + offset.x, Props.maxBarrierOffset.x), Props.maxBarrierOffset.y), Mathf.Max(Mathf.Min(barrierOffset.z + offset.z, Props.maxBarrierOffset.z), 1));
            }
            if (size == IntVec2.Invalid)
            {
                rectSize = barrierSize;
            }
            else
            {
                rectSize = new IntVec2(barrierSize.x + size.x, barrierSize.z + size.z);
                if (rectSize.x > Props.maxBarrierSize.x && rectSize.z > Props.maxBarrierSize.z && rectSize.x < 1 && rectSize.z < 1)
                {
                    return false;
                }
            }
            CellRect rect = new CellRect(rectOffset.x, rectOffset.z, rectSize.x, rectSize.z);
            return rect.minX <= 0 && rect.maxX >= 0 && rect.minZ > 0 && rect.Count() <= Props.maxArea;
        }

        public Rot4 DirectionRotated(Rot4 direction)
        {
            switch (parent.Rotation.AsInt)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        switch (direction.AsInt)
                        {
                            case 0:
                                {
                                    direction = Rot4.East;
                                    break;
                                }
                            case 1:
                                {
                                    direction = Rot4.South;
                                    break;
                                }
                            case 2:
                                {
                                    direction = Rot4.West;
                                    break;
                                }
                            case 3:
                                {
                                    direction = Rot4.North;
                                    break;
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (direction.AsInt)
                        {
                            case 0:
                                {
                                    direction = Rot4.South;
                                    break;
                                }
                            case 1:
                                {
                                    direction = Rot4.West;
                                    break;
                                }
                            case 2:
                                {
                                    direction = Rot4.North;
                                    break;
                                }
                            case 3:
                                {
                                    direction = Rot4.East;
                                    break;
                                }
                        }
                        break;
                    }
                case 3:
                    {
                        switch (direction.AsInt)
                        {
                            case 0:
                                {
                                    direction = Rot4.West;
                                    break;
                                }
                            case 1:
                                {
                                    direction = Rot4.North;
                                    break;
                                }
                            case 2:
                                {
                                    direction = Rot4.East;
                                    break;
                                }
                            case 3:
                                {
                                    direction = Rot4.South;
                                    break;
                                }
                        }
                        break;
                    }
            }
            return direction;
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
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    FloatMenuOption floatMenuOptionEast = new FloatMenuOption(DirectionRotated(Rot4.East).ToStringWord(), delegate
                    {
                        ChangeBarrier(new IntVec2(1, 0), IntVec2.Invalid);
                    });
                    floatMenuOptionEast.Disabled = !CanChangeBarrier(new IntVec2(1, 0), IntVec2.Invalid);
                    if (floatMenuOptionEast.Disabled)
                    {
                        floatMenuOptionEast.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionEast);
                    FloatMenuOption floatMenuOptionWest = new FloatMenuOption(DirectionRotated(Rot4.West).ToStringWord(), delegate
                    {
                        ChangeBarrier(new IntVec2(-1, 0), IntVec2.Invalid);
                    });
                    floatMenuOptionWest.Disabled = !CanChangeBarrier(new IntVec2(-1, 0), IntVec2.Invalid);
                    if (floatMenuOptionWest.Disabled)
                    {
                        floatMenuOptionWest.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionWest);
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
                    FloatMenuOption floatMenuOptionEast = new FloatMenuOption($"+{DirectionRotated(Rot4.East).ToStringWord()}", delegate
                    {
                        ChangeBarrier(IntVec2.Invalid, new IntVec2(1, 0));
                    });
                    floatMenuOptionEast.Disabled = !CanChangeBarrier(IntVec2.Invalid, new IntVec2(1, 0));
                    if (floatMenuOptionEast.Disabled)
                    {
                        floatMenuOptionEast.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionEast);
                    FloatMenuOption floatMenuOptionWest = new FloatMenuOption($"-{DirectionRotated(Rot4.East).ToStringWord()}", delegate
                    {
                        ChangeBarrier(IntVec2.Invalid, new IntVec2(-1, 0));
                    });
                    floatMenuOptionWest.Disabled = !CanChangeBarrier(IntVec2.Invalid, new IntVec2(-1, 0));
                    if (floatMenuOptionWest.Disabled)
                    {
                        floatMenuOptionWest.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionWest);
                    FloatMenuOption floatMenuOptionNorth = new FloatMenuOption($"+{DirectionRotated(Rot4.North).ToStringWord()}", delegate
                    {
                        ChangeBarrier(IntVec2.Invalid, new IntVec2(0, 1));
                    });
                    floatMenuOptionNorth.Disabled = !CanChangeBarrier(IntVec2.Invalid, new IntVec2(0, 1));
                    if (floatMenuOptionNorth.Disabled)
                    {
                        floatMenuOptionNorth.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionNorth);
                    FloatMenuOption floatMenuOptionSouth = new FloatMenuOption($"-{DirectionRotated(Rot4.North).ToStringWord()}", delegate
                    {
                        ChangeBarrier(IntVec2.Invalid, new IntVec2(0, -1));
                    });
                    floatMenuOptionSouth.Disabled = !CanChangeBarrier(IntVec2.Invalid, new IntVec2(0, -1));
                    if (floatMenuOptionSouth.Disabled)
                    {
                        floatMenuOptionSouth.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionSouth);
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Size.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.VacBarrierRoofPojector.Gizmo.Size.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport"),
                Order = 30,
            };
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("ComplementaryOdyssey.VacBarrierRoofPojector.InspectString.BarrierTiles".Translate(barrierTiles.Count(), Props.maxArea));
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