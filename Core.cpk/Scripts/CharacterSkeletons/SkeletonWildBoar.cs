namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonWildBoar : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.5;

        public override Vector2D IconOffset => (5, -45);

        public override double IconScale => 0.6;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("WildBoar/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("WildBoar/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/Boar";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, 0.01 * scaleMultiplier);
            shadowRenderer.Scale = 0.75 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.25,
                                center: (0, 0.125))
                .AddShapeCircle(radius: 0.45,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.45,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}