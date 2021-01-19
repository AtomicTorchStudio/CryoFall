namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class ClientComponentNightVisionEffect2 : ClientComponentNightVisionEffect
    {
        protected override EffectResource EffectResource
            => new("PostEffects/NightVision2");
    }
}