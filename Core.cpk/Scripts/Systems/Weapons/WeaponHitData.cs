namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public readonly struct WeaponHitData : IRemoteCallParameter
    {
        public readonly IWorldObject WorldObject;

        public WeaponHitData(IWorldObject worldObject)
        {
            this.WorldObject = worldObject;
        }
    }
}