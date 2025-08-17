using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace ComplementaryOdyssey
{
    public class Building_VacDoor : Building_SupportedDoor
    {

        public CompPowerTrader PowerTrader => powerTraderCached ?? (powerTraderCached = GetComp<CompPowerTrader>());
        private CompPowerTrader powerTraderCached;

        public VacuumComponent Vacuum => vacuumCached ?? (vacuumCached = base.MapHeld?.GetComponent<VacuumComponent>());
        private VacuumComponent vacuumCached;

        public Graphic graphicSub
        {
            get
            {
                if (graphicSubCached == null)
                {
                    graphicSubCached = Graphic.data.attachments.FirstOrDefault().Graphic;
                }
                return graphicSubCached;
            }
        }
        private Graphic graphicSubCached;

        public bool BarrierOn
        {
            get
            {
                if (PowerTrader.PowerOn)
                {
                    return base.Map.Biome.inVacuum;
                }
                return false;
            }
        }

        public override bool ExchangeVacuum => !IsAirtight || (Open && !PowerTrader.PowerOn);

        protected override float TempEqualizeRate
        {
            get
            {
                if (!PowerTrader.PowerOn)
                {
                    return base.TempEqualizeRate;
                }
                return 0f;
            }
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff")
            {
                Vacuum?.Dirty();
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (BarrierOn)
            {
                graphicSub?.Draw(drawLoc, flip ? base.Rotation.Opposite : base.Rotation, this);
            }
            base.DrawAt(drawLoc, flip);
        }
    }
}