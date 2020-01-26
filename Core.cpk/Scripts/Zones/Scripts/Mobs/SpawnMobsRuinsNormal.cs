namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMobsRuinsNormal : ProtoZoneSpawnScript
    {
        protected override double MaxSpawnAttempsMultiplier => 10;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                .Add(GetTrigger<TriggerWorldInit>())
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(SpawnRuinsConstants.SpawnInterval));

            var lizard = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                  .Add<MobCloakedLizard>()
                                  .SetCustomPaddingWithSelf(18);

            var crawler = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                   .Add<MobCrawler>()
                                   .SetCustomPaddingWith(lizard, 18)
                                   .SetCustomPaddingWithSelf(18);

            var scorpion = spawnList.CreatePreset(interval: 20, padding: 0.5)
                                    .Add<MobScorpion>()
                                    .SetCustomPaddingWith(lizard,  18)
                                    .SetCustomPaddingWith(crawler, 18)
                                    .SetCustomPaddingWithSelf(18);
        }
    }
}