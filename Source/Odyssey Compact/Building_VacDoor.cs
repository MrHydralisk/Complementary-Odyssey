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

namespace OdysseyCompact
{
    public class Building_VacDoor : Building_SupportedDoor
    {

        private CompPowerTrader PowerTrader => powerTraderCached ?? (powerTraderCached = GetComp<CompPowerTrader>());
        private CompPowerTrader powerTraderCached;

        private VacuumComponent Vacuum => vacuumCached ?? (vacuumCached = base.MapHeld?.GetComponent<VacuumComponent>());
        private VacuumComponent vacuumCached;

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
            if (PowerTrader.PowerOn && base.Map.Biome.inVacuum)
            {
                Graphic.Draw(drawLoc, flip ? base.Rotation.Opposite : base.Rotation, this);
            }
        }
    }
}