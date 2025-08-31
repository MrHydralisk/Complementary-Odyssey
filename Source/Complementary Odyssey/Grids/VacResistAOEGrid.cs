using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class VacResistAOEGrid : ICellBoolGiver
    {
        private const float LineSpacing = 29f;

        private Map map;

        private CellBoolDrawer drawer;

        private short[] vacResistAOEGrid;
        private int initializedTick = -1;

        public Color Color => Color.white;

        public VacResistAOEGrid(Map map)
        {
            this.map = map;
            vacResistAOEGrid = new short[map.cellIndices.NumGridCells];
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3640, 1f);
        }

        public void UpdateGrid(List<IntVec3> cells, bool isActivated = true)
        {
            int offset = (isActivated ? +1 : -1);
            foreach (IntVec3 cell in cells)
            {
                int index = map.cellIndices.CellToIndex(cell);
                vacResistAOEGrid[index] = (short)Mathf.Max(vacResistAOEGrid[index] + offset, 0);
                map.regionGrid.GetValidRegionAt_NoRebuild(cell)?.District.Notify_RoofChanged();
            }
            SetDirty();
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
            bool isRender = false;
            Thing SelectedThing = Find.Selector.FirstSelectedObject as Thing;
            if (SelectedThing != null)
            {
                CompVacResistAOEProjector compVacResistAOEProjector = SelectedThing.TryGetComp<CompVacResistAOEProjector>();
                if ((compVacResistAOEProjector != null))
                {
                    isRender = true;
                }
            }
            if (isRender)
            {
                MarkForDraw();
            }
        }

        public bool GetCellBool(IntVec3 cell)
        {
            return GetCellBool(map.cellIndices.CellToIndex(cell));
        }

        public bool GetCellBool(int index)
        {
            return (ushort)vacResistAOEGrid[index] > 0;
        }

        public Color GetCellExtraColor(int index)
        {
            return Color.blue.ToTransparent(0.25f);
        }

        public void InitialActivated(bool isActivated = true)
        {
            if (isActivated)
            {
                for (int i = 0; i < vacResistAOEGrid.Length; i++)
                {
                    vacResistAOEGrid[i] = (short)(vacResistAOEGrid[i] + 5);
                }
                initializedTick = Find.TickManager.TicksGame;
            }
            else
            {
                for (int i = 0; i < vacResistAOEGrid.Length; i++)
                {
                    vacResistAOEGrid[i] = (short)Mathf.Max(vacResistAOEGrid[i] - 5, 0);
                }
                initializedTick = -2;
            }
        }

        public void Tick()
        {
            if (initializedTick == -1)
            {
                InitialActivated();
            }
            else if (initializedTick > -1 && Find.TickManager.TicksGame - initializedTick > COMod.Settings.VacRoofPoweredAfterLanding)
            {
                InitialActivated(false);
            }
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                SetDirty();
            }
        }
    }
}