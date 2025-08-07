using RimWorld;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class MapComponent_CompOdyssey : MapComponent
    {
        public SurfaceResourceGrid surfaceResourceGrid;

        private static MapComponent_CompOdyssey currentInstance;

        public MapComponent_CompOdyssey(Map map) : base(map)
        {
            surfaceResourceGrid = new SurfaceResourceGrid(map);
        }

        public static MapComponent_CompOdyssey CachedInstance(Map map)
        {
            if (currentInstance == null || currentInstance.map != map)
            {
                currentInstance = map.GetComponent<MapComponent_CompOdyssey>();
            }
            return currentInstance;
        }

        public override void MapComponentDraw()
        {
            base.MapComponentDraw();
            if (map == Find.CurrentMap)
            {
                surfaceResourceGrid.SurfaceResourceGridUpdate();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref surfaceResourceGrid, "surfaceResourceGrid", map);
        }
    }
}