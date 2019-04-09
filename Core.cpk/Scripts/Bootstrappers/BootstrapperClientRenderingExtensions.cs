namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.AmbientOcclusion;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperClientRenderingExtensions : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            Api.Client.Scene.CreateSceneObject("Ambient Occlusion Renderer")
               .AddComponent<ClientComponentAmbientOcclusionRenderer>();

            Api.Client.Scene.CreateSceneObject("Lighting Renderer")
               .AddComponent<ClientComponentLightingRenderer>();
        }
    }
}