namespace AtomicTorch.CBND.CoreMod.Systems.Physics
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum CollisionGroupId : byte
    {
        Default = 0,

        HitboxMelee = 1,

        HitboxRanged = 2,

        ClickArea = 3,

        InteractionArea = 4
    }
}