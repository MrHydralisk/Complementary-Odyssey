using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

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

        private int bridgeTilesDeployed => bridgeDepTiles.Count();
        private TerrainGrid terrainGrid => parent.Map.terrainGrid;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
            parent.Map.events.TerrainChanged += Notify_OnTerrainChanged;
        }

        private void Notify_OnTerrainChanged(IntVec3 cell)
        {
            int index = bridgeDepTiles.IndexOf(cell);
        }

        public virtual List<IntVec3> bridgeTiles(Rot4 rotation)
        {
            return Props.bridgeTiles().Select((IntVec3 iv3) => parent.Position + iv3.RotatedBy(rotation)).ToList();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned || !(compPowerTrader?.PowerOn ?? true))
            {
                return;
            }
            if (isDeploying && Find.TickManager.TicksGame >= tickNextDeploy)
            {
                bool isNotDeployed = true;
                if (bridgeTilesDeployed < Props.maxDeploy)
                {
                    IntVec3 cell = bridgeTiles(parent.Rotation)[bridgeTilesDeployed];
                    if (GenConstruct.CanBuildOnTerrain(Props.deployableTerrain, cell, parent.Map, Rot4.North))
                    {
                        terrainGrid.SetFoundation(cell, Props.deployableTerrain);
                        bridgeDepTiles.Add(cell);
                        tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
                        isNotDeployed = false;
                    }
                }
                if (isNotDeployed)
                {
                    isDeploying = false;
                    tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging;
                }
            }
            else if (isPacking && Find.TickManager.TicksGame >= tickNextPacking)
            {
                bool isNotPacked = true;
                if (bridgeTilesDeployed > 0)
                {
                    if (terrainGrid.FoundationAt(bridgeDepTiles.Last()) == Props.deployableTerrain)
                    {
                        terrainGrid.RemoveFoundation(bridgeDepTiles.Last());
                        bridgeDepTiles.RemoveAt(bridgeTilesDeployed - 1);
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

        //public bool CanDeploy(IntVec3 tile)
        //{
        //    List<Thing> list = parent.Map.thingGrid.ThingsListAt(tile);
        //    bool isCanDeploy = true;
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        if (list[i] is Building building)
        //        {
        //            isCanDeploy = isCanDeploy && ((building == null) || (building == parent) || ((building.def != Props.deployableThing) && !building.def.IsEdifice()));
        //        }
        //        if (list[i].def.category == ThingCategory.Plant && list[i].def.blockWind)
        //        {
        //            isCanDeploy = false;
        //        }
        //        if (!isCanDeploy)
        //        {
        //            break;
        //        }
        //    }
        //    return isCanDeploy;
        //}

        //public void Notify_SolarPanelDestroyed(Building_SolarArrayPanel solarPanel)
        //{
        //    bridgeTiles.Remove(solarPanel);
        //    if (isDeploying)
        //    {
        //        isDeploying = false;
        //        tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging * 4;
        //    }
        //}

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.events.TerrainChanged -= Notify_OnTerrainChanged;
            for (int i = bridgeTilesDeployed - 1; i >= 0; i--)
            {
                if (terrainGrid.FoundationAt(bridgeDepTiles[i]) == Props.deployableTerrain)
                {
                    terrainGrid.RemoveFoundation(bridgeDepTiles[i]);
                }
            }
            base.PostDeSpawn(map, mode);
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
                    FloatMenuOption floatMenuOptionEast = new FloatMenuOption($"{Rot4.North.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.North;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    floatMenuOptionEast.Disabled = parent.Rotation == Rot4.North;
                    if (floatMenuOptionEast.Disabled)
                    {
                        floatMenuOptionEast.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionEast);
                    FloatMenuOption floatMenuOptionWest = new FloatMenuOption($"{Rot4.East.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.East;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    floatMenuOptionWest.Disabled = parent.Rotation == Rot4.East;
                    if (floatMenuOptionWest.Disabled)
                    {
                        floatMenuOptionWest.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionWest);
                    FloatMenuOption floatMenuOptionNorth = new FloatMenuOption($"{Rot4.South.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.South;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    floatMenuOptionNorth.Disabled = parent.Rotation == Rot4.South;
                    if (floatMenuOptionNorth.Disabled)
                    {
                        floatMenuOptionNorth.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionNorth);
                    FloatMenuOption floatMenuOptionSouth = new FloatMenuOption($"{Rot4.West.ToStringWord()}", delegate
                    {
                        parent.Rotation = Rot4.West;
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                    });
                    floatMenuOptionSouth.Disabled = parent.Rotation == Rot4.West;
                    if (floatMenuOptionSouth.Disabled)
                    {
                        floatMenuOptionSouth.Label += $" {"Disabled".Translate()}";
                    }
                    floatMenuOptions.Add(floatMenuOptionSouth);
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "Rotate"/*.Translate()*/,
                defaultDesc = "ComplementaryOdyssey.VacBarrierProjector.Gizmo.Size.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight"),
                Order = 30,
                Disabled = bridgeDepTiles.Count() > 0,
                disabledReason = "Already deployed"
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
            Scribe_Values.Look(ref isDeploying, "isDeploying", false);
            Scribe_Values.Look(ref isPacking, "isPacking", false);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("ComplementaryOdyssey.Deployable.InspectString.Deployed".Translate(bridgeTilesDeployed, 0, Props.maxDeploy));
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