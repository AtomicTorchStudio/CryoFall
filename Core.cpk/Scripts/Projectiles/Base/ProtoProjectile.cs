namespace AtomicTorch.CBND.CoreMod.Projectiles
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Projectiles;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Base class for projectile types with states.
    /// </summary>
    /// <typeparam name="TPrivateState">Type of server private state.</typeparam>
    /// <typeparam name="TPublicState">Type of server public state.</typeparam>
    /// <typeparam name="TClientState">Type of client state.</typeparam>
    public abstract class ProtoProjectile
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoWorldObject
          <IProjectile,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoProjectile
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
    }

    /// <summary>
    /// Base class for projectile types without states.
    /// </summary>
    [NoDoc]
    public abstract class ProtoProjectile
        : ProtoProjectile<EmptyPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}