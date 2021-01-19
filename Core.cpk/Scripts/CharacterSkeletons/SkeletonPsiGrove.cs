namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonPsiGrove : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0;

        public override Vector2D IconOffset => (0, -60);

        public override double IconScale => 0.62;

        public override SkeletonResource SkeletonResourceBack
            => null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("PsiGrove/Front");

        public override double WorldScale => 0.5;

        protected override string SoundsFolderPath => "Skeletons/PsiGrove";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            if (shadowRenderer.SceneObject?.AttachedWorldObject?.ProtoGameObject
                    is ObjectCorpse)
            {
                shadowRenderer.IsEnabled = false;
                return;
            }

            shadowRenderer.PositionOffset = (0, -0.025 * scaleMultiplier);
            shadowRenderer.Scale = (1 * scaleMultiplier, 1.5 * scaleMultiplier);
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.IsNotPushable = true;

            physicsBody
                .AddShapeCircle(radius: 0.185, center: (0, 0))
                .AddShapeCircle(radius: 0.15,  center: (-0.16, -0.025))
                .AddShapeCircle(radius: 0.15,  center: (0.16, -0.025))
                .AddShapeRectangle(size: (0.3, 1.1), offset: (-0.3 / 2.0, 0), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.3, 1.2), offset: (-0.3 / 2.0, 0.05), group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            if (skeleton.SceneObject?.AttachedWorldObject?.ProtoGameObject
                    is ObjectCorpse)
            {
                skeleton.DrawOrder = DrawOrder.FloorCharredGround + 1;
                return;
            }

            skeleton.DrawOrderOffsetY = -0.15;
            
            var sceneObject = skeleton.SceneObject;
            if (sceneObject.AttachedWorldObject is not null)
            {
                ClientLighting.CreateLightSourceSpot(
                    sceneObject,
                    color: LightColors.PragmiumLuminescenceNode,
                    size: (3, 5.5),
                    spritePivotPoint: (0.5, 0.5),
                    positionOffset: (0, 0.8));
            }
        }
    }
}