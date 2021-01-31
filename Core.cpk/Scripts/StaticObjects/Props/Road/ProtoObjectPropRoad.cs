namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectPropRoad : ProtoObjectProp, IProtoObjectMovementSurface
    {
        public double CharacterMoveSpeedMultiplier => 1.15;

        public GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override StaticObjectKind Kind => StaticObjectKind.Platform;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrder = DrawOrder.Floor;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }
    }
}