namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [NotPersistent]
    [NotNetworkAvailable]
    public enum CompletionistPageName : byte
    {
        [Description(CoreStrings.WindowCompletionist_TabFood)]
        Food = 0,

        [Description(CoreStrings.WindowCompletionist_TabCreatures)]
        Creatures = 1,

        [Description(CoreStrings.WindowCompletionist_TabLoot)]
        Loot = 2,

        [Description(CoreStrings.WindowCompletionist_TabFish)]
        Fish = 3,

        [Description(CoreStrings.WindowCompletionist_TabEvents)]
        Events = 4
    }
}