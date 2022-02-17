namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonBroodNest : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0;

        public override Vector2D IconOffset => (10, 0);

        public override double IconScale => 0.32;

        public override SkeletonResource SkeletonResourceBack
            => null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("BroodNest/Front");

        public override double WorldScale => 0.45;

        protected override string SoundsFolderPath => "Skeletons/Spitter"; // reusing the sounds from Spitter

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.IsEnabled = false;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.IsNotPushable = true;

            physicsBody
                .AddShapeCircle(radius: 0.55, center: (-0.5, 0.2))
                .AddShapeCircle(radius: 0.55, center: (0.5, 0.2))
                .AddShapeRectangle(size: (1.2, 1.2), offset: (-0.6, -0.4))
                .AddShapeCircle(radius: 0.7, center: (0, 0.4), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.7, center: (0, 0.4), group: CollisionGroups.HitboxRanged);
        }
    }
}