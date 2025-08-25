using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    [StaticConstructorOnStartup]
    public class CompExtBridge : ThingComp
    {
        public CompProperties_ExtBridge Props => (CompProperties_ExtBridge)props;

        public CompPowerTrader compPowerTrader => compPowerTraderCached ?? (compPowerTraderCached = parent.TryGetComp<CompPowerTrader>());
        private CompPowerTrader compPowerTraderCached;

        public List<IntVec3> bridgeDepTiles = new List<IntVec3>();

        public int tickNextDeploy = -1;
        public int tickNextPacking = -1;
        public int tickNextRecharging = -1;
        public bool isDeploying;
        public bool isPacking;
        public int maxDeploy;

        private int bridgeTilesDeployed => bridgeDepTiles.Count();
        private TerrainGrid terrainGrid => parent.Map.terrainGrid;
        public bool PowerOn => compPowerTrader?.PowerOn ?? true;

        public override void PostPostMake()
        {
            base.PostPostMake();
            maxDeploy = Props.maxDeploy;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
            parent.Map.events.TerrainChanged += Notify_OnTerrainChanged;
        }

        private void Notify_OnTerrainChanged(IntVec3 cell)
        {
            int index = bridgeDepTiles.IndexOf(cell);
            if (index > -1)
            {
                bridgeDepTiles.RemoveAt(index);
            }
        }

        public virtual List<IntVec3> bridgeTiles(Rot4 rotation)
        {
            return Props.bridgeTiles(maxDeploy).Select((IntVec3 iv3) => parent.Position + iv3.RotatedBy(rotation)).ToList();
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
                List<IntVec3> bTiles = bridgeTiles(parent.Rotation);
                int i = 0;
                while (i < maxDeploy)
                {
                    IntVec3 cell = bTiles[i];
                    if (bridgeDepTiles.Contains(cell))
                    {
                        i++;
                        continue;
                    }
                    else if (CanDeploy(cell))
                    {
                        terrainGrid.SetFoundation(cell, Props.deployableTerrain);
                        bridgeDepTiles.Add(cell);
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
                bridgeDepTiles.Sort((IntVec3 a, IntVec3 b) => a.DistanceTo(parent.Position).CompareTo(b.DistanceTo(parent.Position)));
            }
            else if (isPacking && Find.TickManager.TicksGame >= tickNextPacking)
            {
                bool isNotPacked = true;
                if (bridgeTilesDeployed > 0)
                {
                    IntVec3 lastTile = bridgeDepTiles.Last();
                    if (terrainGrid.FoundationAt(lastTile) == Props.deployableTerrain)
                    {
                        if (lastTile.GetFirstPawn(parent.Map) == null)
                        {
                            terrainGrid.RemoveFoundation(lastTile);
                            tickNextPacking = Find.TickManager.TicksGame + Props.ticksPerPacking;
                            isNotPacked = false;
                        }
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
            return cell.InBounds(parent.Map) && GenConstruct.CanBuildOnTerrain(Props.deployableTerrain, cell, parent.Map, Rot4.North);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.events.TerrainChanged -= Notify_OnTerrainChanged;
            for (int i = bridgeTilesDeployed - 1; i >= 0; i--)
            {
                if (map.terrainGrid.FoundationAt(bridgeDepTiles[i]) == Props.deployableTerrain)
                {
                    map.terrainGrid.RemoveFoundation(bridgeDepTiles[i]);
                }
                bridgeDepTiles.RemoveAt(bridgeTilesDeployed - 1);
            }
            base.PostDeSpawn(map, mode);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Gizmo_SetMaxExtBridgeTiles(this);
            yield return new Command_Action
            {
                action = delegate
                {
                    isDeploying = true;
                },
                defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Label".Translate(),
                defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Desc".Translate(Props.deployableTerrain.label),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter"),
                Order = 30,
                Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableTerrain.label) : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableTerrain.label) : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
            };
            if (bridgeTilesDeployed > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        isPacking = true;
                    },
                    defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Label".Translate(),
                    defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Desc".Translate(Props.deployableTerrain.label),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter"),
                    Order = 30,
                    Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                    disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableTerrain.label) : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableTerrain.label) : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
                };
            }
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
                Disabled = bridgeDepTiles.Count() > 0 || !PowerOn,
                disabledReason = PowerOn ? "ComplementaryOdyssey.ExtBridge.Gizmo.Rotate.Reason.Deployed".Translate() : "NoPower".Translate()
            };
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
            Scribe_Collections.Look(ref bridgeDepTiles, "bridgeDepTiles", LookMode.Value);
            Scribe_Values.Look(ref tickNextDeploy, "tickNextDeploy", -1);
            Scribe_Values.Look(ref tickNextPacking, "tickNextPacking", -1);
            Scribe_Values.Look(ref tickNextRecharging, "tickNextRecharging", -1);
            Scribe_Values.Look(ref maxDeploy, "maxDeploy", Props.maxDeploy);
            Scribe_Values.Look(ref isDeploying, "isDeploying", false);
            Scribe_Values.Look(ref isPacking, "isPacking", false);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("ComplementaryOdyssey.ExtBridge.InspectString.Deployed".Translate(bridgeTilesDeployed, maxDeploy, Props.maxDeploy));
            if (isDeploying)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableTerrain.label));
            }
            else if (isPacking)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableTerrain.label));
            }
            else if (Find.TickManager.TicksGame < tickNextRecharging)
            {
                inspectStrings.Add("ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod());
            }
            return String.Join("\n", inspectStrings);
        }
    }
}