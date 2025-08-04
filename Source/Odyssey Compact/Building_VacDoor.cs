using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Reflection;
using RimWorld;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using System.Drawing;

namespace OdysseyCompact
{
    public class Building_VacDoor : Building_SupportedDoor
    {

        public CompPowerTrader PowerTrader => powerTraderCached ?? (powerTraderCached = GetComp<CompPowerTrader>());
        private CompPowerTrader powerTraderCached;

        public VacuumComponent Vacuum => vacuumCached ?? (vacuumCached = base.MapHeld?.GetComponent<VacuumComponent>());
        private VacuumComponent vacuumCached;

        public GraphicData graphicDataSub => graphicDataSubCached ?? (graphicDataSubCached = Graphic.data.attachments.FirstOrDefault());
        private GraphicData graphicDataSubCached;

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
            base.DrawAt(drawLoc, flip); 
            if (graphicDataSub != null && PowerTrader.PowerOn && base.Map.Biome.inVacuum)
            {
                graphicDataSub.Graphic.Draw(drawLoc + graphicDataSub.drawOffset, flip ? base.Rotation.Opposite : base.Rotation, this);
            }
        }
    }
}