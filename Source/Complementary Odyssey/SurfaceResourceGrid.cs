using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace ComplementaryOdyssey
{
    public class SurfaceResourceGrid : ICellBoolGiver, IExposable
    {
        private const float LineSpacing = 29f;

        private const float IconPaddingRight = 4f;

        private const float IconSize = 27f;

        private Map map;

        private CellBoolDrawer drawer;

        private bool[] oreGrid;
        private Dictionary<int, Mineable> oreDict = new Dictionary<int, Mineable>();

        public Color Color => Color.white;

        public SurfaceResourceGrid(Map map)
        {
            this.map = map;
            oreGrid = new bool[map.cellIndices.NumGridCells];
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3640, 1f);
        }

        public void ExposeData()
        {
            MapExposeUtility.ExposeUshort(map, (IntVec3 c) => (oreGrid[map.cellIndices.CellToIndex(c)] ? (ushort)1 : (ushort)0), delegate (IntVec3 c, ushort val)
            {
                oreGrid[map.cellIndices.CellToIndex(c)] = val == 1;
            }, "oreGrid");
        }

        public Mineable OreAt(IntVec3 c)
        {
            Mineable ore = null;
            int index = map.cellIndices.CellToIndex(c);
            if (oreGrid[index])
            {
                if (!oreDict.TryGetValue(index, out ore))
                {
                    List<Thing> list = map.thingGrid.ThingsListAt(c);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is Mineable mineable)
                        {
                            if (mineable.def.building.mineableThing != null)
                            {
                                ore = mineable;
                                oreDict.Add(index, ore);
                            }
                            break;
                        }
                    }
                    if (ore == null)
                    {
                        oreGrid[index] = false;
                    }
                }
            }
            return ore;
        }

        public void SetAt(IntVec3 c)
        {
            int index = map.cellIndices.CellToIndex(c);
            if (!oreGrid[index])
            {
                oreGrid[index] = true;
                drawer.SetDirty();
            }
        }

        public void SurfaceResourceGridUpdate()
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

        public void SurfaceResourcesOnGUI()
        {
            Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
            if (singleSelectedThing != null)
            {
                CompShortRangeMineralScanner compShortRangeMineralScanner = singleSelectedThing.TryGetComp<CompShortRangeMineralScanner>();
                if ((compShortRangeMineralScanner != null) && AnyActiveShortRangeMineralScannersOnMap())
                {
                    RenderMouseAttachments();
                }
            }
        }

        public bool AnyActiveShortRangeMineralScannersOnMap()
        {
            foreach (Building item in map.listerBuildings.allBuildingsColonist)
            {
                CompShortRangeMineralScanner compShortRangeMineralScanner = item.TryGetComp<CompShortRangeMineralScanner>();
                if (compShortRangeMineralScanner != null && compShortRangeMineralScanner.ShouldShowSurfaceResourceOverlay())
                {
                    return true;
                }
            }
            return false;
        }

        private void RenderMouseAttachments()
        {
            IntVec3 c = UI.MouseCell();
            if (!c.InBounds(map))
            {
                return;
            }
            Mineable ore = OreAt(c);
            if (ore != null)
            {
                Vector2 vector = c.ToVector3().MapToUIPosition();
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                float num2 = (UI.CurUICellSize() - 27f) / 2f;
                Rect rect = new Rect(vector.x + num2, vector.y - UI.CurUICellSize() + num2, 27f, 27f);
                Widgets.ThingIcon(rect, ore.def.building.mineableThing);
                Widgets.Label(new Rect(rect.xMax + 4f, rect.y, 999f, 29f), $"{ore.def.building.EffectiveMineableYield} {ore.Label}" /*"DeepResourceRemaining".Translate(NamedArgumentUtility.Named(thingDef, "RESOURCE"), num.Named("COUNT"))*/);
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public bool GetCellBool(int index)
        {
            return OreAt(map.cellIndices.IndexToCell(index)) != null;
        }

        public Color GetCellExtraColor(int index)
        {
            IntVec3 c = map.cellIndices.IndexToCell(index);
            Mineable ore = OreAt(c);
            return DebugMatsSpectrum.Mat(Mathf.RoundToInt((float)ore.HitPoints / ore.MaxHitPoints / 2f * 100f) % 100, transparent: true).color;
        }
    }
}