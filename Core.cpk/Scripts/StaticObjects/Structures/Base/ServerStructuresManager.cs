namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerStructuresManager
    {
        public delegate void StructureDelegate(IStaticWorldObject structure);

        public static event StructureDelegate StructureDestroyed;

        public static void NotifyObjectDestroyed(IStaticWorldObject structure)
        {
            Api.SafeInvoke(() => StructureDestroyed?.Invoke(structure));
        }
    }
}