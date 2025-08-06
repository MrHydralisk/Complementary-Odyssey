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

        private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        public List<IntVec3> solarTiles = new List<IntVec3>();
        public List<Building_SolarArrayPanel> solarPanels = new List<Building_SolarArrayPanel>();
        public int SolarPanelsTotal => Props.zoneTiles().Count();

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
            foreach (IntVec3 tile in solarTiles)
            {
                TryDeploySolarPanel(tile);
            }
        }

        public bool TryDeploySolarPanel(IntVec3 tile)
        {
            Building building = tile.GetFirstBuilding(parent.Map);
            if (!parent.Map.roofGrid.Roofed(tile) && ((building == null) || (building == parent)))
            {
                Building_SolarArrayPanel solarPanel = ThingMaker.MakeThing(Props.deployableThing) as Building_SolarArrayPanel;
                solarPanel.solarArray = parent;
                solarPanel.SetFactionDirect(parent.Faction);
                GenSpawn.Spawn(solarPanel, tile, parent.Map, parent.Rotation);
                solarPanels.Add(solarPanel);
                return true;
            }
            return false;
        }

        public void Notify_SolarPanelDestroyed(Building_SolarArrayPanel solarPanel)
        {
            solarPanels.Remove(solarPanel);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
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

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.CompInspectStringExtra());
            inspectStrings.Add($"Deployed {SolarPanelsDeployed}[{SolarPanelsAvailable}]/{SolarPanelsTotal} Panels");
            return String.Join("\n", inspectStrings);
        }
    }
}