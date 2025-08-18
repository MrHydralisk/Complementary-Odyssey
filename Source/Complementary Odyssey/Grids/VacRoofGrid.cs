using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class VacRoofGrid : ICellBoolGiver
    {
        private const float LineSpacing = 29f;

        private Map map;

        private CellBoolDrawer drawer;

        private short[] powerGrid;
        private bool[] roofGrid;

        public Color Color => Color.white;

        public VacRoofGrid(Map map)
        {
            this.map = map;
            powerGrid = new short[map.cellIndices.NumGridCells];
            roofGrid = new bool[map.cellIndices.NumGridCells];
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3640, 1f);
        }

        public void UpdatePowerGrid(List<IntVec3> cells, bool isPowerOn = true)
        {
            int offset = (isPowerOn ? +1 : -1);
            foreach (IntVec3 cell in cells)
            {
                int index = map.cellIndices.CellToIndex(cell);
                powerGrid[index] = (short)Mathf.Max(powerGrid[index] + offset, 0);
                map.regionGrid.GetValidRegionAt_NoRebuild(cell)?.District.Notify_RoofChanged();
            }
            SetDirty();
        }

        public bool VacRoofAt(int index, out RoofDef roofDef)
        {
            roofDef = map.roofGrid.RoofAt(index);
            bool isVacRoof = roofDef?.IsVacRoof(out _) ?? false;
            if (isVacRoof && !roofGrid[index])
            {
                roofGrid[index] = isVacRoof;
                drawer.SetDirty();
            }
            return isVacRoof;
        }

        public void SetDirty()
        {
            drawer.SetDirty();
        }

        public void GridUpdate()
        {
            drawer.CellBoolDrawerUpdate();
        }

        public void MarkForDraw()
        {
            if (map == Find.CurrentMap && !Find.ScreenshotModeHandler.Active)
            {
                drawer.MarkForDraw();
            }
        }

        public void GridOnGUI()
        {
            Thing singleSelectedThing = Find.Selector.FirstSelectedObject as Thing;
            if (singleSelectedThing != null)
            {
                CompVacBarrierRoofProjector compVacBarrierRoofPojector = singleSelectedThing.TryGetComp<CompVacBarrierRoofProjector>();
                if ((compVacBarrierRoofPojector != null))
                {
                    MarkForDraw();
                    RenderMouseAttachments();
                }
            }
        }

        private void RenderMouseAttachments()
        {
            IntVec3 c = UI.MouseCell();
            if (!c.InBounds(map))
            {
                return;
            }
            if (VacRoofAt(map.cellIndices.CellToIndex(c), out RoofDef roofDef) && !GetPoweredCellBool(c))
            {
                Vector2 vector = c.ToVector3().MapToUIPosition();
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(vector.x, vector.y, 999f, LineSpacing), "NoPower".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public bool GetCellBool(IntVec3 cell)
        {
            return GetCellBool(map.cellIndices.CellToIndex(cell));
        }

        public bool GetCellBool(int index)
        {
            return VacRoofAt(index, out _) || GetPoweredCellBool(index);
        }

        public bool GetPoweredCellBool(IntVec3 cell)
        {
            return GetPoweredCellBool(map.cellIndices.CellToIndex(cell));
        }

        public bool GetPoweredCellBool(int index)
        {
            return (ushort)powerGrid[index] > 0;
        }

        public Color GetCellExtraColor(int index)
        {
            Color color = Color.white;
            RoofDef roofDef = map.roofGrid.RoofAt(index);
            if (roofDef != null && ComplementaryOdysseyUtility.IsVacRoof(roofDef, out ComplementaryOdysseyDefModExtension modExtension))
            {
                color = modExtension.color;
            }
            return color.ToTransparent(powerGrid[index] > 0 ? 0.5f : 0.25f);
        }
    }
}