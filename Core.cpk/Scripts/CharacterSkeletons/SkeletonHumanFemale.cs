namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonHumanFemale : SkeletonHuman
    {
        // WIP, use this to toggle female skeleton on/off
        public const bool IsFemaleSkeletonEnabled = false;

        public override SkeletonResource SkeletonResourceBack { get; }
            = IsFemaleSkeletonEnabled
                  ? new SkeletonResource("Human/FemaleBack")
                  : new SkeletonResource("Human/MaleBack");

        public override SkeletonResource SkeletonResourceFront { get; }
            = IsFemaleSkeletonEnabled
                  ? new SkeletonResource("Human/FemaleFront")
                  : new SkeletonResource("Human/MaleFront");

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.06 * scaleMultiplier);
            shadowRenderer.Scale = 0.7 * scaleMultiplier;
        }
    }
}