namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerItemUseObserver
    {
        public delegate void ItemUsedDelegate(ICharacter character, IItem item);

        public static event ItemUsedDelegate ItemUsed;

        public static void NotifyItemUsed(ICharacter character, IItem item)
        {
            if (character is null)
            {
                Api.Logger.Error("Character is null for " + nameof(NotifyItemUsed));
                return;
            }

            if (item is null)
            {
                Api.Logger.Error("Item is null for " + nameof(NotifyItemUsed));
                return;
            }

            Api.SafeInvoke(() => ItemUsed?.Invoke(character, item));
        }
    }
}