namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class DefaultMineralClientState
        : StaticObjectClientState, IClientStateWithShadowRenderer
    {
        public IComponentSpriteRenderer RendererShadow { get; set; }
    }
}