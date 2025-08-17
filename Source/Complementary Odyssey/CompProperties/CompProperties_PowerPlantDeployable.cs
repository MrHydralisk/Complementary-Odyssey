using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class CompProperties_PowerPlantDeployable : CompProperties_Power
    {
        public GraphicData graphicDataSub;
        public ThingDef deployableThing;
        public int ticksPerDeploy = 0;
        public int ticksPerPacking = 0;
        public int ticksPerRecharging = 2500;
        public float powerOutputVacMult = 0;
        public Vector3 powerBarOffset = Vector3.zero;
        public Vector3? powerBarOffsetNorth;
        public Vector3? powerBarOffsetEast;
        public Vector3? powerBarOffsetSouth;
        public Vector3? powerBarOffsetWest;
        public Vector2 powerBarSize = Vector2.one;
        public float powerBarMargin = 0.02f;
        public IntVec2 zoneSize = IntVec2.One;
        public List<IntVec3> zoneAdditionalTiles = new List<IntVec3>();
        public IntVec3 zoneOffset = IntVec3.Zero;

        public virtual List<IntVec3> zoneTiles()
        {
            List<IntVec3> tiles = new CellRect(zoneOffset.x, zoneOffset.z, zoneSize.x, zoneSize.z).Cells.ToList();
            tiles.AddRange(zoneAdditionalTiles);
            return tiles;
        }

        public Vector3 PowerBarOffsetForRot(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    {
                        return powerBarOffsetNorth ?? powerBarOffset;
                    }
                case 1:
                    {
                        return powerBarOffsetEast ?? powerBarOffset;
                    }
                case 2:
                    {
                        return powerBarOffsetSouth ?? powerBarOffset;
                    }
                case 3:
                    {
                        return powerBarOffsetWest ?? powerBarOffset;
                    }
                default:
                    {
                        return powerBarOffset;
                    }
            }
        }
    }
}