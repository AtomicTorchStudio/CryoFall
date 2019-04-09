namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class VegetationClientState : StaticObjectClientState
    {
        public byte LastGrowthStage { get; set; }

        public IComponentSpriteRenderer RendererShadow { get; set; }
    }
}