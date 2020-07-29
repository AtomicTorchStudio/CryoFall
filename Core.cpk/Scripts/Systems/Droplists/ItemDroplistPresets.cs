namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public static class ItemDroplistPresets
    {
        public static readonly IReadOnlyDropItemsListPreset Gemstones
            = new DropItemsListPreset(
                  outputs: 1,
                  probability: 1 / 1000.0,
                  useGuaranteedProbabilityAlgorithm: true,
                  storageKey: "Gemstones")
              .Add<ItemGemDiamond>()
              .Add<ItemGemEmerald>()
              .Add<ItemGemRuby>()
              .Add<ItemGemSapphire>()
              .Add<ItemGemTopaz>()
              .Add<ItemGemTourmaline>();

        public static readonly IReadOnlyDropItemsListPreset GoldNuggets
            = new DropItemsListPreset(
                    outputs: 1,
                    probability: 1 / 50.0,
                    useGuaranteedProbabilityAlgorithm: true,
                    storageKey: "GoldNuggets")
                .Add<ItemGoldNugget>(1, countRandom: 3);

        public static readonly IReadOnlyDropItemsListPreset GoldNuggetsRare
            = new DropItemsListPreset(
                    outputs: 1,
                    probability: 1 / 100.0,
                    useGuaranteedProbabilityAlgorithm: true,
                    storageKey: "GoldNuggetsRare")
                .Add<ItemGoldNugget>(1, countRandom: 3);
    }
}