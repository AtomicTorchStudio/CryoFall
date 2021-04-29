namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum MobSpawnState : byte
    {
        Spawned = 0,

        Spawning = 1,

        Despawning = 2
    }
}