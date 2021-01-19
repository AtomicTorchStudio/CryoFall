namespace AtomicTorch.CBND.CoreMod.Drones
{
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class DroneIndustrialAdvanced : ProtoDrone<ItemDroneIndustrialAdvanced>
    {
        public override Vector2D BeamOriginOffset => (0, 0.5);

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override float ObjectSoundRadius => 1;

        public override double PhysicsBodyAccelerationCoef => 3;

        public override double PhysicsBodyFriction => 30;

        public override double StatMoveSpeed => 4.0;

        public override float StructurePointsMax => 300;

        protected override double DrawVerticalOffset => 0.88;

        protected override SoundResource EngineSoundResource { get; }
            = new("Items/Drones/FlyAdvanced");

        protected override double EngineSoundVolume => 0.6;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0, 0.1 + this.DrawVerticalOffset);
        }

        protected override void PrepareProtoDrone(out IProtoItemWeapon protoTool)
        {
            protoTool = Api.GetProtoEntity<ItemDroneIndustrialAdvancedTool>();
        }

        protected override void PrepareProtoVehicleDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters)
        {
            damageRadius = 2.1;
            explosionPreset = ExplosionPresets.Large;

            damageDescriptionCharacters = new DamageDescription(
                damageValue: 50,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: damageRadius,
                damageDistribution: new DamageDistribution(DamageType.Kinetic, 1));
        }

        protected override void SharedCreatePhysicsDrone(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.6, 0.6), offset: (-0.3, 0.5), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.6), offset: (-0.3, 0.5), group: CollisionGroups.HitboxRanged);
        }
    }
}