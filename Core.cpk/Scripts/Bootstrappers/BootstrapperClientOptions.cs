namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.ClientOptions;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(BootstrapperClientCore))]
    public class BootstrapperClientOptions : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            foreach (var protoOptionTab in Api.FindProtoEntities<ProtoOptionsCategory>())
            {
                protoOptionTab.LoadOptionsFromStorage();
            }
        }
    }
}