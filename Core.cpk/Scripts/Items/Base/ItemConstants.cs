namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ItemConstants
    {
        public static bool ServerPvpIsFullLootEnabled { get; private set; }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                ServerPvpIsFullLootEnabled
                    = ServerRates.Get(
                          "PvpIsFullLootEnabled",
                          defaultValue: 0,
                          @"For PvP servers you can enable full loot (dropping the equipped items on death)
                            or keep it by default (not dropping the equipped items on death).")
                      != 0;
            }
        }
    }
}