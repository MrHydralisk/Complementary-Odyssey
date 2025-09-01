using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class Gizmo_SetBorderRoofRetractor : Gizmo
    {
        private Texture2D barTex;

        private Texture2D barHighlightTex;

        private Texture2D barDragTex;

        public CompRoofRetractor roofRetractor;

        private Vector2 targetValuePct;

        private bool initialized;

        protected Rect barRect;

        private const float Spacing = 8f;

        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D BarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D DragBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

        protected virtual float Width => 160f;

        protected Vector2 Target
        {
            get
            {
                return new Vector2((float)(roofRetractor.borders.x - roofRetractor.Props.Borders.x) / (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x), (float)(roofRetractor.borders.z - roofRetractor.Props.Borders.x) / (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x));
            }
            set
            {
                roofRetractor.borders = new IntVec2((int)(value.x * (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x) + roofRetractor.Props.Borders.x), (int)(value.y * (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x) + roofRetractor.Props.Borders.x));
            }
        }

        protected Vector2 ValuePercent
        {
            get
            {
                return new Vector2((float)(roofRetractor.borders.x - roofRetractor.Props.Borders.x) / (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x), (float)(roofRetractor.borders.z - roofRetractor.Props.Borders.x) / (roofRetractor.Props.Borders.z - roofRetractor.Props.Borders.x));
            }
        }

        protected virtual Color BarColor { get; }

        protected virtual Color BarHighlightColor { get; }

        protected virtual Color BarDragColor { get; }

        protected virtual FloatRange DragRange { get; } = FloatRange.ZeroToOne;

        protected string BarLabel
        {
            get
            {
                return "ComplementaryOdyssey.RoofRetractor.Command.SetBorderRoofRetractor.BarLabel".Translate(roofRetractor.borders.x, roofRetractor.borders.z, roofRetractor.Props.Borders.x, roofRetractor.Props.Borders.z);
            }
        }

        protected string Title => "ComplementaryOdyssey.RoofRetractor.Command.SetBorderRoofRetractor.Title".Translate();

        protected virtual int Increments { get; } = 20;


        protected virtual string HighlightTag => null;

        protected bool[] DraggingBar
        {
            get
            {
                if (draggingBar.NullOrEmpty())
                {
                    draggingBar = new bool[2];
                }
                return draggingBar;
            }
            set
            {
                draggingBar = value;
            }
        }
        private static bool[] draggingBar;

        public override float Order
        {
            get
            {
                return -100f;
            }
            set
            {
                base.Order = value;
            }
        }

        public sealed override float GetWidth(float maxWidth)
        {
            return Width;
        }

        protected virtual IEnumerable<float> GetBarThresholds()
        {
            yield break;
        }

        public Gizmo_SetBorderRoofRetractor(CompRoofRetractor roofRetractor)
        {
            this.roofRetractor = roofRetractor;
        }

        protected string GetTooltip()
        {
            return "";
        }

        private void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                targetValuePct.x = Mathf.Clamp(Target.x, DragRange.min, DragRange.max);
                targetValuePct.y = Mathf.Clamp(Target.y, DragRange.min, DragRange.max);
                barTex = ((BarColor == default(Color)) ? BarTex : SolidColorMaterials.NewSolidColorTexture(BarColor));
                barHighlightTex = ((BarHighlightColor == default(Color)) ? BarHighlightTex : SolidColorMaterials.NewSolidColorTexture(BarHighlightColor));
                barDragTex = ((BarDragColor == default(Color)) ? DragBarTex : SolidColorMaterials.NewSolidColorTexture(BarDragColor));
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (!initialized)
            {
                Initialize();
            }
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(8f);
            Widgets.DrawWindowBackground(rect);
            bool mouseOverElement = false;
            Text.Font = GameFont.Small;
            Rect headerRect = rect2;
            headerRect.height = Text.LineHeight;
            DrawHeader(headerRect, ref mouseOverElement);
            barRect = rect2;
            barRect.yMin = headerRect.yMax + 4f;
            barRect.SplitHorizontally(barRect.height / 2, out Rect topRect, out Rect bottomRect);
            Vector2 targetValuePctOld = new Vector2(targetValuePct.x, targetValuePct.y);
            bool[] draggingBar = DraggingBar;
            Widgets.DraggableBar(topRect, barTex, barHighlightTex, EmptyBarTex, barDragTex, ref draggingBar[0], ValuePercent.x, ref targetValuePct.x, GetBarThresholds(), Increments, DragRange.min, DragRange.max);
            Widgets.DraggableBar(bottomRect, barTex, barHighlightTex, EmptyBarTex, barDragTex, ref draggingBar[1], ValuePercent.y, ref targetValuePct.y, GetBarThresholds(), Increments, DragRange.min, DragRange.max);
            DraggingBar = draggingBar;
            if (targetValuePct != targetValuePctOld)
            {
                if (targetValuePct.x != Target.x)
                {
                    targetValuePct.x = Mathf.Clamp(targetValuePct.x, DragRange.min, DragRange.max);
                    targetValuePct.y = Mathf.Clamp(targetValuePct.y, targetValuePct.x, DragRange.max);
                }
                else if (targetValuePct.y != Target.y)
                {
                    targetValuePct.y = Mathf.Clamp(targetValuePct.y, DragRange.min, DragRange.max);
                    targetValuePct.x = Mathf.Clamp(targetValuePct.x, DragRange.min, targetValuePct.y);
                }
                Target = targetValuePct;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, BarLabel);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect) && !mouseOverElement)
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8573612));
            }
            if (!HighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, HighlightTag);
            }
            return new GizmoResult(GizmoState.Clear);
        }

        protected virtual void DrawHeader(Rect headerRect, ref bool mouseOverElement)
        {
            string title = Title;
            title = title.Truncate(headerRect.width);
            Widgets.Label(headerRect, title);
        }
    }
}