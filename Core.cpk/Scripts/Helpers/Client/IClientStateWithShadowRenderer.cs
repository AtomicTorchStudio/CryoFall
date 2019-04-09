namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public interface IClientStateWithShadowRenderer : IClientState
    {
        IComponentSpriteRenderer RendererShadow { get; set; }
    }
}