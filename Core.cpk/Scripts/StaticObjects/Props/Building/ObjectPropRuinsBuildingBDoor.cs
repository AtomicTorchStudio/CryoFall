namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Building
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropRuinsBuildingBDoor : ProtoObjectProp
    {
        public override ObjectSoundMaterial ObjectSoundMaterial
            => ObjectSoundMaterial.Metal;

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 2), offset: (0, 0));
        }
    }
}