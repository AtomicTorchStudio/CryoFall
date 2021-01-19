namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonHumanFemale : ProtoCharacterSkeletonHuman
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("Human/FemaleBack");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Human/FemaleFront");

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.06 * scaleMultiplier);
            shadowRenderer.Scale = 0.7 * scaleMultiplier;
        }
    }
}