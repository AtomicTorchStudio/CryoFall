namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonCloakedLizard : ProtoCharacterSkeletonAnimal
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("CloakedLizard/CloakedLizardBack");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("CloakedLizard/CloakedLizardFront");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/CloakedLizard";

        protected override double VolumeFootsteps => 1;

        public override void ClientSetupShadowRenderer(
            IComponentSpriteRenderer shadowRenderer,
            double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.11 * scaleMultiplier);
            shadowRenderer.Scale = 0.9 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.AddShapeRectangle(
                size: (0.7, 0.3),
                offset: (-0.35, -0.15),
                group: CollisionGroups.Default);

            physicsBody.AddShapeCircle(
                radius: 0.5,
                center: (0, 0.35),
                group: CollisionGroups.HitboxMelee);

            physicsBody.AddShapeCircle(
                radius: 0.5,
                center: (0, 0.35),
                group: CollisionGroups.HitboxRanged);
        }
    }
}