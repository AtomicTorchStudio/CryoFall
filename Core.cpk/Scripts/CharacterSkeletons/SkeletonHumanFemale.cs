namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonHumanFemale : SkeletonHuman
    {
        // sorry, dear women, we have not completed female skeletons yet! :-(
        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("Human/CharacterMaleBack");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Human/CharacterMaleFront");

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.06 * scaleMultiplier);
            shadowRenderer.Scale = 0.7 * scaleMultiplier;
        }
    }
}