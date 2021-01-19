namespace AtomicTorch.CBND.CoreMod.Objects
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerObjectUseObserver
    {
        public delegate void ObjectUsedDelegate(ICharacter character, IWorldObject worldObject);

        public static event ObjectUsedDelegate ObjectUsed;

        public static void NotifyObjectUsed(ICharacter character, IWorldObject worldObject)
        {
            if (worldObject is null)
            {
                Api.Logger.Error("Object is null for " + nameof(NotifyObjectUsed));
                return;
            }

            Api.SafeInvoke(() => ObjectUsed?.Invoke(character, worldObject));
        }
    }
}