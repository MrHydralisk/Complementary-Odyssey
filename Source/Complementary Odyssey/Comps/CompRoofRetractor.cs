using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    [StaticConstructorOnStartup]
    public class CompRoofRetractor : ThingComp
    {
        public CompProperties_RoofRetractor Props => (CompProperties_RoofRetractor)props;

        public CompPowerTrader compPowerTrader => compPowerTraderCached ?? (compPowerTraderCached = parent.TryGetComp<CompPowerTrader>());
        private CompPowerTrader compPowerTraderCached;

        public List<IntVec3> roofRetTiles = new List<IntVec3>();

        public int tickNextDeploy = -1;
        public int tickNextPacking = -1;
        public int tickNextRecharging = -1;
        public bool isDeploying;
        public bool isPacking;
        public IntVec2 borders;

        private int roofTilesRetracted => roofRetTiles.Count();
        private RoofGrid roofGrid => parent.Map.roofGrid;
        private AreaManager areaManager => parent.Map.areaManager;
        public bool PowerOn => compPowerTrader?.PowerOn ?? true;

        public override void PostPostMake()
        {
            base.PostPostMake();
            borders = Props.Borders;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
            parent.Map.events.RoofChanged += Notify_OnRoofChanged;
        }

        private void Notify_OnRoofChanged(IntVec3 cell)
        {
            int index = roofRetTiles.IndexOf(cell);
            if (index > -1 && roofGrid.Roofed(cell))
            {
                roofRetTiles.RemoveAt(index);
            }
        }

        public virtual List<IntVec3> roofTiles(Rot4 rotation)
        {
            return Props.roofTiles(borders).Select((IntVec3 iv3) => parent.Position + iv3.RotatedBy(rotation)).ToList();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned || !PowerOn)
            {
                return;
            }
            if (isDeploying && Find.TickManager.TicksGame >= tickNextDeploy)
            {
                bool isNotDeployed = true;
                List<IntVec3> bTiles = roofTiles(parent.Rotation);
                int i = bTiles.Count;
                while (i >= 0)
                {
                    IntVec3 cell = bTiles[i];
                    if (roofRetTiles.Contains(cell))
                    {
                        i--;
                        continue;
                    }
                    else if (CanDeploy(cell))
                    {
                        areaManager.BuildRoof[cell] = false;
                        areaManager.NoRoof[cell] = true;
                        roofGrid.SetRoof(cell, null);
                        RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(new List<IntVec3>() { cell }, parent.Map, removalMode: true);
                        roofRetTiles.Add(cell);
                        tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
                        isNotDeployed = false;
                    }
                    break;
                }
                if (isNotDeployed)
                {
                    isDeploying = false;
                    tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging;
                }
                roofRetTiles.Sort((IntVec3 a, IntVec3 b) => a.DistanceTo(parent.Position).CompareTo(b.DistanceTo(parent.Position)));
            }
            else if (isPacking && Find.TickManager.TicksGame >= tickNextPacking)
            {
                bool isNotPacked = true;
                if (roofTilesRetracted > 0)
                {
                    IntVec3 firstTile = roofRetTiles.First();

                    if (!roofGrid.Roofed(firstTile) && areaManager.NoRoof[firstTile] == true)
                    {
                        areaManager.BuildRoof[firstTile] = true;
                        areaManager.NoRoof[firstTile] = false;
                        roofGrid.SetRoof(firstTile, RoofDefOf.RoofConstructed);
                        MoteMaker.PlaceTempRoof(firstTile, parent.Map);
                        tickNextPacking = Find.TickManager.TicksGame + Props.ticksPerPacking;
                        isNotPacked = false;
                    }
                }
                if (isNotPacked)
                {
                    isPacking = false;
                    tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging;
                }
            }
        }

        public bool CanDeploy(IntVec3 cell)
        {
            return cell.InBounds(parent.Map) && roofGrid.RoofAt(cell) == RoofDefOf.RoofConstructed;
        }

        public void SelectNeighbour()
        {
            List<ThingWithComps> visited = new List<ThingWithComps>();
            List<CompRoofRetractor> neighbours = new List<CompRoofRetractor>();
            neighbours.Add(this);
            visited.Add(parent);
            int iteration = 0;
            while (neighbours.Count > 0 && iteration < 200)
            {
                CompRoofRetractor current = neighbours.FirstOrDefault();
                if (current == null)
                {
                    break;
                }
                Find.Selector.Select(current.parent);
                neighbours.RemoveAt(0);
                foreach (IntVec3 cell in GenAdjFast.AdjacentCells8Way(current.parent.Position))
                {
                    foreach (Thing thing in cell.GetThingList(current.parent.Map))
                    {
                        CompRoofRetractor compRoofRetractor = thing.TryGetComp<CompRoofRetractor>();
                        if (compRoofRetractor != null && !visited.Contains(thing) && thing.Rotation == parent.Rotation)
                        {
                            neighbours.Add(compRoofRetractor);
                            visited.Add(compRoofRetractor.parent);
                        }
                    }
                }
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.events.RoofChanged -= Notify_OnRoofChanged;
            base.PostDeSpawn(map, mode);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Gizmo_SetBorderRoofRetractor(this);
            if (roofTilesRetracted > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        isPacking = true;
                    },
                    defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Label".Translate(),
                    defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter"),
                    Order = 30,
                    Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                    disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate() : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate() : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
                };
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    isDeploying = true;
                },
                defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter"),
                Order = 30,
                Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate() : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate() : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    FloatMenuOption floatMenuOptionNorth = new FloatMenuOption($"{Rot4.North.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.North;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    if (parent.Rotation == Rot4.North)
                    {
                        floatMenuOptionNorth.Disabled = true;
                        floatMenuOptionNorth.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Current".Translate();
                    }
                    else if (!CanDeploy(parent.Position + Rot4.North.AsIntVec3))
                    {
                        floatMenuOptionNorth.Disabled = true;
                        floatMenuOptionNorth.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Obstructed".Translate();
                    }
                    floatMenuOptions.Add(floatMenuOptionNorth);
                    FloatMenuOption floatMenuOptionEast = new FloatMenuOption($"{Rot4.East.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.East;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    if (parent.Rotation == Rot4.East)
                    {
                        floatMenuOptionEast.Disabled = true;
                        floatMenuOptionEast.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Current".Translate();
                    }
                    else if (!CanDeploy(parent.Position + Rot4.East.AsIntVec3))
                    {
                        floatMenuOptionEast.Disabled = true;
                        floatMenuOptionEast.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Obstructed".Translate();
                    }
                    floatMenuOptions.Add(floatMenuOptionEast);
                    FloatMenuOption floatMenuOptionSouth = new FloatMenuOption($"{Rot4.South.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.South;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    if (parent.Rotation == Rot4.South)
                    {
                        floatMenuOptionSouth.Disabled = true;
                        floatMenuOptionSouth.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Current".Translate();
                    }
                    else if (!CanDeploy(parent.Position + Rot4.South.AsIntVec3))
                    {
                        floatMenuOptionSouth.Disabled = true;
                        floatMenuOptionSouth.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Obstructed".Translate();
                    }
                    floatMenuOptions.Add(floatMenuOptionSouth);
                    FloatMenuOption floatMenuOptionWest = new FloatMenuOption($"{Rot4.West.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.West;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    if (parent.Rotation == Rot4.West)
                    {
                        floatMenuOptionWest.Disabled = true;
                        floatMenuOptionWest.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Current".Translate();
                    }
                    else if (!CanDeploy(parent.Position + Rot4.West.AsIntVec3))
                    {
                        floatMenuOptionWest.Disabled = true;
                        floatMenuOptionWest.Label += "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Obstructed".Translate();
                    }
                    floatMenuOptions.Add(floatMenuOptionWest);
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight"),
                Order = 30,
                Disabled = roofRetTiles.Count() > 0 || !PowerOn,
                disabledReason = PowerOn ? "ComplementaryOdyssey.RoofRetractor.Gizmo.Rotate.Reason.Retracted".Translate() : "NoPower".Translate()
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    SelectNeighbour();
                },
                defaultLabel = "SelectAllLinked".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/ShowColonistBar"),
                Order = 30
            };
            foreach (Gizmo item in RoofRetractorSettingsClipboard.CopyPasteGizmosFor(this))
            {
                yield return item;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        tickNextRecharging = -1;
                    },
                    defaultLabel = "Dev: Reset cooldown",
                    defaultDesc = "Reset cooldown between activation"
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref roofRetTiles, "bridgeDepTiles", LookMode.Value);
            Scribe_Values.Look(ref tickNextDeploy, "tickNextDeploy", -1);
            Scribe_Values.Look(ref tickNextPacking, "tickNextPacking", -1);
            Scribe_Values.Look(ref tickNextRecharging, "tickNextRecharging", -1);
            Scribe_Values.Look(ref borders, "maxDeploy", Props.Borders);
            Scribe_Values.Look(ref isDeploying, "isDeploying", false);
            Scribe_Values.Look(ref isPacking, "isPacking", false);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("ComplementaryOdyssey.RoofRetractor.InspectString.Retracted".Translate(roofTilesRetracted, borders.z - borders.x + 1, Props.Borders.z - Props.Borders.x + 1));
            if (isDeploying)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(RoofDefOf.RoofConstructed.label));
            }
            else if (isPacking)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(RoofDefOf.RoofConstructed.label));
            }
            else if (Find.TickManager.TicksGame < tickNextRecharging)
            {
                inspectStrings.Add("ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod());
            }
            return String.Join("\n", inspectStrings);
        }
    }
}