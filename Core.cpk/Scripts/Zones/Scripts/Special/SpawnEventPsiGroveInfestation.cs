namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;

    public class SpawnEventPsiGroveInfestation : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            // no triggers, only manual triggering through the event when it starts
            spawnList.CreatePreset(interval: 10, padding: 0.5, useSectorDensity: false)
                     .AddExact<MobPsiGrove>()
                     .SetCustomPaddingWithSelf(10);
        }
    }
}