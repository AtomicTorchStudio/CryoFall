namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMobsRuinsNormal : ProtoZoneSpawnScript
    {
        protected override double MaxSpawnAttemptsMultiplier => 10;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                .Add(GetTrigger<TriggerWorldInit>())
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(SpawnRuinsConstants.SpawnInterval));

            var lizard = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                  .AddExact<MobCloakedLizard>()
                                  .SetCustomPaddingWithSelf(18);

            var crawler = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                   .AddExact<MobCrawler>()
                                   .SetCustomPaddingWith(lizard, 18)
                                   .SetCustomPaddingWithSelf(18);

            var scorpion = spawnList.CreatePreset(interval: 20, padding: 0.5)
                                    .AddExact<MobScorpion>()
                                    .SetCustomPaddingWith(lizard,  18)
                                    .SetCustomPaddingWith(crawler, 18)
                                    .SetCustomPaddingWithSelf(18);

            // spawn only few mutants in radtowns and central town to make a rare surprise
            var mutants = spawnList.CreatePreset(interval: 80, padding: 0.5, useSectorDensity: false)
                                   .AddExact<MobMutantBoar>()
                                   .AddExact<MobMutantHyena>()
                                   .AddExact<MobMutantWolf>()
                                   .SetCustomPaddingWith(lizard,   8)
                                   .SetCustomPaddingWith(crawler,  8)
                                   .SetCustomPaddingWith(scorpion, 8)
                                   // very large padding with self to prevent spawning mutants nearby
                                   .SetCustomPaddingWithSelf(79);
        }
    }
}