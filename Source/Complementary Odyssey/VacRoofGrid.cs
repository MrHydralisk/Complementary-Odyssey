using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class VacRoofGrid : ICellBoolGiver, IExposable
    {
        private Map map;

        private VacRoofStatus[] statusGrid;

        public Color Color => Color.white;

        public VacRoofGrid(Map map)
        {
            this.map = map;
            statusGrid = new VacRoofStatus[map.cellIndices.NumGridCells];
        }

        public void ExposeData()
        {
            MapExposeUtility.ExposeUshort(map, (IntVec3 c) => (ushort)statusGrid[map.cellIndices.CellToIndex(c)], delegate (IntVec3 c, ushort val)
            {
                statusGrid[map.cellIndices.CellToIndex(c)] = (VacRoofStatus)val;
            }, "statusGrid");
        }

        public VacRoofStatus GetCellStatus(int index)
        {
            return statusGrid[index];
        }

        public bool GetCellBool(IntVec3 cell)
        {
            return (ushort)statusGrid[map.cellIndices.CellToIndex(cell)] > 0;
        }

        public bool GetCellBool(int index)
        {
            return (ushort)statusGrid[index] > 0;
        }

        public Color GetCellExtraColor(int index)
        {
            return Color.blue.ToTransparent((ushort)statusGrid[index] / 2);
        }
    }
}