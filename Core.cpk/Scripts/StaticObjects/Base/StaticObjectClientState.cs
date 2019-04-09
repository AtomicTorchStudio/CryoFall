namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class StaticObjectClientState : BaseClientState, IClientStateWithObjectRenderer
    {
        public IComponentSpriteRenderer Renderer { get; set; }

        public BaseClientComponentLightSource RendererLight { get; set; }

        public IComponentSpriteRenderer RendererOcclusion { get; set; }

        public IComponentSoundEmitter SoundEmitter { get; set; }
    }
}