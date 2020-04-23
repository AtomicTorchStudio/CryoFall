namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonCloakedLizard : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.5;

        public override Vector2D IconOffset => (0, -40);

        public override double IconScale => 0.5;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("CloakedLizard/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("CloakedLizard/Front");

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
            physicsBody
                .AddShapeRectangle(size: (0.7, 0.3),
                                   offset: (-0.35, -0.15));

            // melee hitbox
            var meleeHitboxHeight = 0.7;
            var meleeHitboxOffset = 0.25;
            physicsBody.AddShapeRectangle(
                size: (0.7, meleeHitboxHeight),
                offset: (-0.35, meleeHitboxOffset),
                group: CollisionGroups.HitboxMelee);

            // ranged hitbox
            var rangedHitboxHeight = 1.1;
            var rangedHitboxOffset = 0;
            physicsBody.AddShapeRectangle(
                size: (0.6, rangedHitboxHeight),
                offset: (-0.3, rangedHitboxOffset),
                group: CollisionGroups.HitboxRanged);
        }
    }
}