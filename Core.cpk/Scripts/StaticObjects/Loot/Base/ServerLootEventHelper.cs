namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerLootEventHelper
    {
        public delegate void LootReceivedDelegate(ICharacter character, IStaticWorldObject staticWorldObject);

        public static event LootReceivedDelegate LootReceived;

        public static void OnLootReceived(ICharacter character, IStaticWorldObject worldObject)
        {
            var handler = LootReceived;
            if (handler is null)
            {
                return;
            }

            Api.SafeInvoke(() => handler(character, worldObject));
        }
    }
}