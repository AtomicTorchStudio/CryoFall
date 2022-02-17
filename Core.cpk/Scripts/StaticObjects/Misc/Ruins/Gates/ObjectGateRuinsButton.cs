namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// The gate ruins button opens the nearby ruins gate when pressed.
    /// </summary>
    public class ObjectGateRuinsButton : ProtoObjectRuinsButton
    {
        [NotLocalizable]
        public override string Name => "Ruins gate button";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double OpenedDuration => 10;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject) - (0, 0.3);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.1);
            renderer.DrawOrderOffsetY -= 0.1;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.5, 0.35), offset: (0.25, 0.1))
                .AddShapeRectangle(size: (0.6, 0.65), offset: (0.2, 0.1),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.7, 0.2),  offset: (0.15, 0.8), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.6, 0.8),  offset: (0.2, 0.1),  group: CollisionGroups.ClickArea);
        }
    }
}