namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerStaticObjectDestroyObserver
    {
        public delegate void StructureDestroyedDelegate(ICharacter character, IStaticWorldObject worldObject);

        public static event StructureDestroyedDelegate ObjectDestroyed;

        public static void NotifyObjectDestroyed(ICharacter character, IStaticWorldObject worldObject)
        {
            Api.SafeInvoke(() => ObjectDestroyed?.Invoke(character, worldObject));
        }
    }
}