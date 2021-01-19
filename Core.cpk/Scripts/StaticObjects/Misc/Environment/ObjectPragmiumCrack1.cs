namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPragmiumCrack1 : ProtoObjectMisc
    {
        // define this object as a structure to prevent terrain decals rendered under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        [NotLocalizable]
        public override string Name => "Pragmium fissure";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            ClientLighting.CreateLightSourceSpot(
                data.GameObject.ClientSceneObject,
                color: LightColors.PragmiumLuminescenceNode.WithAlpha(0x66),
                size: (3, 4),
                positionOffset: (0.5, 0.5));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2.0;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }
    }
}