﻿namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonBlackBeetle : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.35;

        public override Vector2D IconOffset => (-5, 15);

        public override double IconScale => 0.75;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("BlackBeetle/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("BlackBeetle/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/Beetle";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.1 * scaleMultiplier);
            shadowRenderer.Scale = 0.6 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.25,
                                center: (0, 0.125))
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}