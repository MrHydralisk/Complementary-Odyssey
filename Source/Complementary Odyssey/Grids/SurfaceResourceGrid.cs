using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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
                bool isFound = oreDict.TryGetValue(index, out ore);
                if (isFound)
                {
                    if (ore.DestroyedOrNull())
                    {
                        oreDict.Remove(index);
                        isFound = false;
                    }
                }
                if (!isFound)
                {
                    List<Thing> list = map.thingGrid.ThingsListAt(c);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is Mineable mineable)
                        {
                            ore = mineable;
                            oreDict.SetOrAdd(index, ore);
                            break;
                        }
                    }
                    if (ore == null)
                    {
                        oreGrid[index] = false;
                        SetDirty();
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
                SetDirty();
            }
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
            Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
            if (singleSelectedThing != null)
            {
                CompTerrainScanner compTerrainScanner = singleSelectedThing.TryGetComp<CompTerrainScanner>();
                if ((compTerrainScanner != null))
                {
                    isRender = true;
                }
            }
            if (!isRender && Find.DesignatorManager.SelectedDesignator is Designator_Mine)
            {
                isRender = true;
            }
            if (isRender && AnyActiveTerrainScannersOnMap())
            {
                MarkForDraw();
                RenderMouseAttachments();
            }
        }

        public bool AnyActiveTerrainScannersOnMap()
        {
            foreach (Building item in map.listerBuildings.allBuildingsColonist)
            {
                CompTerrainScanner compTerrainScanner = item.TryGetComp<CompTerrainScanner>();
                if (compTerrainScanner != null && compTerrainScanner.ShouldShowSurfaceResourceOverlay())
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
                float num2 = (UI.CurUICellSize() - IconSize) / 2f;
                Rect rect = new Rect(vector.x + num2, vector.y - UI.CurUICellSize() + num2, IconSize, IconSize);
                ThingDef oreMine = ore.def.building.mineableThing;
                if (oreMine == null)
                {
                    Widgets.ThingIcon(rect, ore);
                    Widgets.Label(new Rect(rect.xMax + IconPaddingRight, rect.y, 999f, LineSpacing), $"{ore.Label}");
                }
                else
                {
                    Widgets.ThingIcon(rect, ore.def.building.mineableThing);
                    Widgets.Label(new Rect(rect.xMax + IconPaddingRight, rect.y, 999f, LineSpacing), $"{ore.def.building.mineableThing.label} x{ore.def.building.EffectiveMineableYield}");
                }
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
            if (ore != null)
            {
                float transparencyValue = (float)ore.HitPoints / ore.MaxHitPoints;
                ThingDef oreMine = ore.def.building.mineableThing;
                if (oreMine == null) 
                {
                    transparencyValue *= 0.25f;
                }
                else
                {
                    if (ThingCategoryDefOf.Chunks.ContainedInThisOrDescendant(oreMine))
                    {
                        transparencyValue *= 0.5f;
                    }
                    else
                    {
                        transparencyValue *= 0.75f;
                    }
                }
                Color colorOre = ore.DrawColorTwo;
                if (colorOre == Color.white)
                {
                    colorOre = ore.DrawColor;
                }
                return colorOre.ToTransparent(transparencyValue);
            }
            return Color.white.ToTransparent(0);
        }
    }
}