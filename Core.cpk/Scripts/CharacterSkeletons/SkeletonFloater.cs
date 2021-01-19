namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonFloater : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.5;

        public override Vector2D IconOffset => (0, -135);

        public override double IconScale => 0.7;

        public override SkeletonResource SkeletonResourceBack { get; }
            = null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Floater/Front");

        public override double WorldScale => 0.8;

        protected override string SoundsFolderPath => "Skeletons/Floater";

        public override void ClientGetAimingOrientation(
            ICharacter character,
            double angleRad,
            ViewOrientation lastViewOrientation,
            out ViewOrientation viewOrientation,
            out float aimCoef)
        {
            viewOrientation = default;
            aimCoef = 0;
        }

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, 0.01 * scaleMultiplier);
            shadowRenderer.Scale = 1.1 * scaleMultiplier;
            shadowRenderer.Color = Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF);
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.15,
                                center: (0, -0.05))
                .AddShapeRectangle(size: (1, 1.2),
                                   offset: (-0.5, 0.4),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1, 1.6),
                                   offset: (-0.5, 0.2),
                                   group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            // fast attack animation
            skeleton.SetMixDuration(null, "Attack", 0.0333f, 0.15f);
        }
    }
}