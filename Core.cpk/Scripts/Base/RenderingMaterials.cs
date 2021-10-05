namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class RenderingMaterials
    {
        public static readonly RenderingMaterial DefaultDrawEffectWithTransparentBorder
            = Api.IsClient
                  ? RenderingMaterial.Create(new EffectResource("DefaultDrawEffectTransparentBorder"))
                  : null;
    }
}