using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using UnityEngine.Analytics;
using System;

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
        public Vector3 solarDrawOffset;
        public int SolarPanelsTotal;

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

        private int SolarPanelsAvailable => solarTiles.Count((IntVec3 iv3) => !parent.Map.roofGrid.Roofed(iv3));

        private float RoofedPowerOutputFactor => (float)SolarPanelsAvailable / SolarPanelsTotal;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            solarTiles = new List<IntVec3>();
            SolarPanelsTotal = 0;
            foreach (IntVec3 tile in Props.zoneTiles())
            {
                solarTiles.Add(parent.Position + tile.RotatedBy(parent.Rotation));
                SolarPanelsTotal++;
            }
            solarDrawOffset = new Vector3(0.5f, parent.DrawPos.y, 0.5f);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            foreach (IntVec3 item in solarTiles)
            {
                if (!item.Roofed(parent.Map))
                {
                    Props.graphicDataSub.Graphic.Draw(item.ToVector3() + solarDrawOffset, flip ? parent.Rotation.Opposite : parent.Rotation, parent);
                }
            }
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
            inspectStrings.Add($"{SolarPanelsAvailable}/{SolarPanelsTotal} Panels");
            return String.Join("\n", inspectStrings);
        }
    }
}