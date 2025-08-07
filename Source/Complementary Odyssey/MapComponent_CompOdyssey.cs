using RimWorld;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class MapComponent_CompOdyssey : MapComponent
    {
        public SurfaceResourceGrid surfaceResourceGrid;

        public MapComponent_CompOdyssey(Map map) : base(map)
        {
            surfaceResourceGrid = new SurfaceResourceGrid(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref surfaceResourceGrid, "surfaceResourceGrid", map);
        }
    }
}