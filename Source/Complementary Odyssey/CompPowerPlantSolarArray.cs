using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    [StaticConstructorOnStartup]
    public class CompPowerPlantSolarArray : CompPowerPlant
    {
        public new CompProperties_PowerPlantDeployable Props => (CompProperties_PowerPlantDeployable)props;

        private const float NightPower = 0f;

        private static readonly Vector2 BarSize = new Vector2(0.77f, 0.046f);

        private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));
        private static readonly Material PowerPlantSolarVacBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.1f, 0.1f));
        private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        public List<IntVec3> solarTiles = new List<IntVec3>();
        private List<IntVec3> checkedTiles = new List<IntVec3>();
        public List<Building_SolarArrayPanel> solarPanels = new List<Building_SolarArrayPanel>();
        public int SolarPanelsTotal => Props.zoneTiles().Count();

        public int tickNextDeploy = -1;
        public int tickNextPacking = -1;
        public int tickNextRecharging = -1;
        public bool isDeploying;
        public bool isPacking;
        public bool isAutoDeploying = true;

        protected override float DesiredPowerOutput
        {
            get
            {
                float po = Mathf.Lerp(0f, 0f - base.Props.PowerConsumption, parent.Map.skyManager.CurSkyGlow);
                if (parent.Map.Biome.inVacuum)
                {
                    po *= Props.powerOutputVacMult;
                }
                po *= RoofedPowerOutputFactor;
                return po;
            }
        }

        private int SolarPanelsAvailable => solarPanels.Count((Thing t) => !t.Map.roofGrid.Roofed(t.Position));
        private int SolarPanelsDeployed => solarPanels.Count();

        private float RoofedPowerOutputFactor => (float)SolarPanelsAvailable / SolarPanelsTotal;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            solarTiles = new List<IntVec3>();
            foreach (IntVec3 tile in Props.zoneTiles())
            {
                solarTiles.Add(parent.Position + tile.RotatedBy(parent.Rotation));
            }
            solarTiles.Sort((IntVec3 a, IntVec3 b) => GetCellAffect(b).CompareTo(GetCellAffect(a)));
            tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
            if (isAutoDeploying)
            {
                isDeploying = true;
            }
        }

        private int GetCellAffect(IntVec3 cell)
        {
            IntVec3 offsetVector = cell - parent.Position;
            return Mathf.Abs(offsetVector.x) + Mathf.Abs(offsetVector.z);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            GenDraw.FillableBarRequest r = default;
            r.center = parent.DrawPos + Props.PowerBarOffsetForRot(parent.Rotation);
            r.size = Props.powerBarSize;
            if (parent.Map.Biome.inVacuum)
            {
                r.fillPercent = base.PowerOutput / (0f - base.Props.PowerConsumption * Props.powerOutputVacMult);
                r.filledMat = PowerPlantSolarVacBarFilledMat;
            }
            else
            {
                r.fillPercent = base.PowerOutput / (0f - base.Props.PowerConsumption);
                r.filledMat = PowerPlantSolarBarFilledMat;
            }
            r.unfilledMat = PowerPlantSolarBarUnfilledMat;
            r.margin = Props.powerBarMargin;
            Rot4 rotation = parent.Rotation;
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (isDeploying && Find.TickManager.TicksGame >= tickNextDeploy)
            {
                List<IntVec3> checkTiles = new List<IntVec3>() { parent.Position };
                bool isNotDeployed = true;
                while (checkTiles.Count > 0)
                {
                    IntVec3 tile = checkTiles[0];
                    checkTiles.RemoveAt(0);
                    checkedTiles.Add(tile);
                    bool b = TryDeploySolarPanel(tile);
                    isNotDeployed = isNotDeployed && !b;
                    if (!isNotDeployed && Find.TickManager.TicksGame < tickNextDeploy)
                    {
                        break;
                    }
                    Building building = tile.GetFirstBuilding(parent.Map);
                    if (building != null && (building == parent || building.def == Props.deployableThing))
                    {
                        foreach (IntVec3 adjTile in GenAdjFast.AdjacentCellsCardinal(tile))
                        {
                            if (checkedTiles.Contains(adjTile) || !solarTiles.Contains(adjTile))
                            {
                                continue;
                            }
                            Building buildingAdj = adjTile.GetFirstBuilding(parent.Map);
                            if ((buildingAdj == null || buildingAdj.def == Props.deployableThing))
                            {
                                checkTiles.AddDistinct(adjTile);
                            }
                        }
                    }
                }
                if (isNotDeployed)
                {
                    isDeploying = false;
                    tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging;
                }
                checkedTiles.Clear();
            }
            else if (isPacking && Find.TickManager.TicksGame >= tickNextPacking)
            {
                bool isNotPacked = true;
                while (solarPanels.Count > 0)
                {
                    solarPanels[0].Destroy();
                    isNotPacked = false;
                    tickNextPacking = Find.TickManager.TicksGame + Props.ticksPerPacking;
                    if (Find.TickManager.TicksGame < tickNextPacking)
                    {
                        break;
                    }
                }
                if (isNotPacked)
                {
                    isPacking = false;
                    tickNextRecharging = Find.TickManager.TicksGame + Props.ticksPerRecharging;
                }
            }
        }

        public bool TryDeploySolarPanel(IntVec3 tile)
        {
            List<Thing> list = parent.Map.thingGrid.ThingsListAt(tile);
            bool isCanDeploy = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Building building)
                {
                    isCanDeploy = isCanDeploy && ((building == null) || (building == parent) || (building.def != Props.deployableThing));
                }
            }
            if (isCanDeploy)
            {
                Building_SolarArrayPanel solarPanel = ThingMaker.MakeThing(Props.deployableThing) as Building_SolarArrayPanel;
                solarPanel.solarArray = parent;
                solarPanel.SetFactionDirect(parent.Faction);
                GenSpawn.Spawn(solarPanel, tile, parent.Map, parent.Rotation);
                solarPanels.Add(solarPanel);
                tickNextDeploy = Find.TickManager.TicksGame + Props.ticksPerDeploy;
                return true;
            }
            return false;
        }

        public void Notify_SolarPanelDestroyed(Building_SolarArrayPanel solarPanel)
        {
            solarPanels.Remove(solarPanel);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = solarPanels.Count() - 1; i >= 0; i--)
            {
                solarPanels[i].Destroy();
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
                defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Desc".Translate(Props.deployableThing.label),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter"),
                Order = 30,
                Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableThing.label) : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableThing.label) : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
            };
            if (solarPanels.Count() > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        isPacking = true;
                        solarPanels.Sort((Building_SolarArrayPanel a, Building_SolarArrayPanel b) => GetCellAffect(b.Position).CompareTo(GetCellAffect(a.Position)));
                    },
                    defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Label".Translate(),
                    defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Desc".Translate(Props.deployableThing.label),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter"),
                    Order = 30,
                    Disabled = isDeploying || isPacking || Find.TickManager.TicksGame < tickNextRecharging,
                    disabledReason = isDeploying ? "ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableThing.label) : isPacking ? "ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableThing.label) : "ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod()
                };
            }
            Command_Toggle command_Toggle = new Command_Toggle();
            command_Toggle.defaultLabel = "ComplementaryOdyssey.Deployable.Gizmo.isAutoDeploying.Label".Translate();
            command_Toggle.defaultDesc = "ComplementaryOdyssey.Deployable.Gizmo.isAutoDeploying.Desc".Translate(Props.deployableThing.label);
            command_Toggle.isActive = () => isAutoDeploying;
            command_Toggle.toggleAction = delegate
            {
                isAutoDeploying = !isAutoDeploying;
            };
            command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
            command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchGravship");
            command_Toggle.hotKey = KeyBindingDefOf.Misc6;
            command_Toggle.Order = 30;
            yield return command_Toggle;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref solarPanels, "solarPanels", LookMode.Reference);
            Scribe_Values.Look(ref tickNextDeploy, "tickNextDeploy", -1);
            Scribe_Values.Look(ref tickNextPacking, "tickNextPacking", -1);
            Scribe_Values.Look(ref tickNextRecharging, "tickNextRecharging", -1);
            Scribe_Values.Look(ref isDeploying, "isDeploying", false);
            Scribe_Values.Look(ref isPacking, "isPacking", false);
            Scribe_Values.Look(ref isAutoDeploying, "isAutoDeploying", true);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.CompInspectStringExtra());
            inspectStrings.Add("ComplementaryOdyssey.Deployable.InspectString.Deployed".Translate(SolarPanelsDeployed, SolarPanelsAvailable, SolarPanelsTotal));
            if (isDeploying)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isDeploying.Reason.Active".Translate(Props.deployableThing.label));
            }
            else if (isPacking)
            {
                inspectStrings.Add("ComplementaryOdyssey.Deployable.Gizmo.isPacking.Reason.Active".Translate(Props.deployableThing.label));
            }
            else if (Find.TickManager.TicksGame < tickNextRecharging)
            {
                inspectStrings.Add("ShieldOnCooldown".Translate() + " " + (tickNextRecharging - Find.TickManager.TicksGame).ToStringTicksToPeriod());
            }
            return String.Join("\n", inspectStrings);
        }
    }
}