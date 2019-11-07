namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerDynamicObjectDestroyObserver
    {
        public delegate void ObjectDestroyedDelegate(ICharacter character, IDynamicWorldObject worldObject);

        public static event ObjectDestroyedDelegate ObjectDestroyed;

        public static void NotifyObjectDestroyed(ICharacter character, IDynamicWorldObject worldObject)
        {
            Api.SafeInvoke(() => ObjectDestroyed?.Invoke(character, worldObject));
        }
    }
}