namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(BootstrapperServerWorld))]
    [PrepareOrder(afterType: typeof(LandClaimSystem.BootstrapperLandClaimSystem))]
    public class BootstrapperWorldInit : BaseBootstrapper
    {
        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            // invoke world init with some small delay because the world objects are not yet loaded
            // (that's the bootstrapper!)
            ServerTimersSystem.AddAction(
                delaySeconds: 0.1,
                () => Api.GetProtoEntity<TriggerWorldInit>().OnWorldInit());
        }
    }
}