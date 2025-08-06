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
    public class CompPowerPlantSolarArray : CompPowerPlant
    {
        public new CompProperties_PowerPlantDeployable Props => (CompProperties_PowerPlantDeployable)props;

        private const float NightPower = 0f;

        private static readonly Vector2 BarSize = new Vector2(0.77f, 0.046f);

        private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

        private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        public List<IntVec3> solarTiles = new List<IntVec3>();
        private List<IntVec3> checkedTiles = new List<IntVec3>();
        public List<Building_SolarArrayPanel> solarPanels = new List<Building_SolarArrayPanel>();
        public int SolarPanelsTotal => Props.zoneTiles().Count();

        public int tickNextDeploy = -1;
        public bool isDeploying;

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
            //foreach (IntVec3 tile in solarTiles)
            //{
            //    TryDeploySolarPanel(tile);
            //}


            isDeploying = true;
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
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = BarSize;
            r.fillPercent = base.PowerOutput / (0f - base.Props.PowerConsumption);
            r.filledMat = PowerPlantSolarBarFilledMat;
            r.unfilledMat = PowerPlantSolarBarUnfilledMat;
            r.margin = 0.05f;
            Rot4 rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
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
                while(checkTiles.Count > 0)
                {
                    IntVec3 tile = checkTiles[0];
                    Log.Message($"{tile} [{checkTiles.Count}]:\n{string.Join("\n", checkTiles)}");
                    checkTiles.RemoveAt(0);
                    checkedTiles.Add(tile);
                    bool b = TryDeploySolarPanel(tile);
                    Log.Message($"{isNotDeployed && !b} = {isNotDeployed} && !{b}");
                    isNotDeployed = isNotDeployed && !b;
                    Log.Message($"{tile} !{isNotDeployed} && {Find.TickManager.TicksGame < tickNextDeploy}");
                    if (!isNotDeployed && Find.TickManager.TicksGame < tickNextDeploy)
                    {
                        break;
                    }
                    Log.Message($"Checking for {tile}");
                    Building building = tile.GetFirstBuilding(parent.Map);
                    if (building != null && (building == parent || building.def == Props.deployableThing))
                    {
                        foreach (IntVec3 adjTile in GenAdjFast.AdjacentCellsCardinal(tile))
                        {
                            Log.Message($"Checking {adjTile} for {tile}\n{checkedTiles.Contains(adjTile)} || !{solarTiles.Contains(adjTile)}");
                            if (checkedTiles.Contains(adjTile) || !solarTiles.Contains(adjTile))
                            {
                                continue;
                            }
                            Building buildingAdj = adjTile.GetFirstBuilding(parent.Map);
                            Log.Message($"Added {adjTile}\n{buildingAdj?.Label} {buildingAdj == null} || {buildingAdj?.def == Props.deployableThing}");
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
                }
                checkedTiles.Clear();
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
                    Log.Message($"{isCanDeploy && ((building == null) || (building == parent) || (building.def != Props.deployableThing))} = {isCanDeploy} && (({building == null}) || ({building == parent}) || ({building.def != Props.deployableThing}))");
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
            for(int i = solarPanels.Count() - 1; i >= 0; i--)
            {
                solarPanels[i].Destroy();
            }
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref solarPanels, "solarPanels", LookMode.Reference);
            Scribe_Values.Look(ref tickNextDeploy, "tickNextDeploy", -1);
            Scribe_Values.Look(ref isDeploying, "isDeploying", false);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.CompInspectStringExtra());
            inspectStrings.Add($"Deployed {SolarPanelsDeployed}[{SolarPanelsAvailable}]/{SolarPanelsTotal} Panels");
            return String.Join("\n", inspectStrings);
        }
    }
}