namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperClientRenderingExtensions : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            Api.Client.Scene.CreateSceneObject("Lighting Renderer")
               .AddComponent<ClientComponentLightingRenderer>();
        }
    }
}