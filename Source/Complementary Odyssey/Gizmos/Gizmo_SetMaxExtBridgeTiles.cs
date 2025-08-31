using RimWorld;
using Verse;

namespace ComplementaryOdyssey
{
    public class Gizmo_SetMaxExtBridgeTiles : Gizmo_Slider
    {
        private CompExtBridge extBridge;

        private static bool draggingBar;

        protected override float Target
        {
            get
            {
                return (float)extBridge.maxDeploy / extBridge.Props.maxDeploy;
            }
            set
            {
                extBridge.maxDeploy = (int)(value * extBridge.Props.maxDeploy);
            }
        }

        protected override float ValuePercent => (float)extBridge.maxDeploy / extBridge.Props.maxDeploy;

        protected override string Title => "ComplementaryOdyssey.ExtBridge.Command.SetMaxExtBridgeTiles.Title".Translate();

        protected override bool IsDraggable => true;

        protected override string BarLabel => extBridge.maxDeploy + " / " + extBridge.Props.maxDeploy;

        protected override bool DraggingBar
        {
            get
            {
                return draggingBar;
            }
            set
            {
                draggingBar = value;
            }
        }

        public Gizmo_SetMaxExtBridgeTiles(CompExtBridge extBridge)
        {
            this.extBridge = extBridge;
        }

        protected override string GetTooltip()
        {
            return "";
        }
    }
}