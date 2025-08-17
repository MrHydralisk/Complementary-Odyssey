using Verse;

namespace ComplementaryOdyssey
{
    public class Building_SolarArrayPanel : Building
    {
        public Thing solarArray;

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            solarArray?.TryGetComp<CompPowerPlantSolarArray>()?.Notify_SolarPanelDestroyed(this);
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref solarArray, "solarArray");
        }
    }
}