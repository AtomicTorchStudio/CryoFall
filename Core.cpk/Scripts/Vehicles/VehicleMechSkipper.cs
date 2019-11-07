namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class VehicleMechSkipper : ProtoVehicleMech
    {
        public override byte CargoItemsSlotsCount => 16;

        public override string Description =>
            "Light design for mechanized battle armor. Boasts relatively high speed while not lacking in armor or firepower.";

        public override ushort EnergyUsePerSecondIdle => 75;

        public override ushort EnergyUsePerSecondMoving => 200;

        public override BaseItemsContainerMechEquipment EquipmentItemsContainerType
            => Api.GetProtoEntity<ContainerMechEquipmentSkipper>();

        public override bool IsPlayersHotbarAndEquipmentItemsAllowed => false;

        public override double MaxDistanceToInteract => 1;

        public override string Name => "Skipper";

        public override double PhysicsBodyAccelerationCoef => 3;

        public override double PhysicsBodyFriction => 10;

        public override double StatMoveSpeed => 2.25;

        public override double StatMoveSpeedRunMultiplier => 1.0; // no run mode

        public override float StructurePointsMax => 600;

        public override double VehicleWorldHeight => 2.0;

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.60,
                kinetic: 0.50,
                heat: 0.40,
                cold: 0.40,
                chemical: 0.60,
                electrical: 0.40,
                radiation: 0.0,
                psi: 0.0);
        }

        protected override void PrepareProtoVehicle(
            InputItems buildRequiredItems,
            InputItems repairStageRequiredItems,
            out int repairStagesCount)
        {
            buildRequiredItems
                .Add<ItemStructuralPlating>(10)
                .Add<ItemUniversalActuator>(8)
                .Add<ItemImpulseEngine>(4)
                .Add<ItemComponentsHighTech>(10);

            repairStageRequiredItems
                .Add<ItemIngotSteel>(10)
                .Add<ItemStructuralPlating>(1)
                .Add<ItemUniversalActuator>(1)
                .Add<ItemComponentsHighTech>(1);

            repairStagesCount = 5;
        }

        protected override void PrepareProtoVehicleLightConfig(ItemLightConfig lightConfig)
        {
            lightConfig.Color = LightColors.ElectricWarm;
            lightConfig.ScreenOffset = (3, -2);
            lightConfig.WorldOffset = (0, -0.5);
            lightConfig.Size = 20;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            base.SharedCreatePhysics(data);

            var physicsBody = data.PhysicsBody;
            if (data.PublicState.PilotCharacter == null)
            {
                // no pilot
                physicsBody.AddShapeRectangle(size: (0.9, 1.5),
                                              offset: (-0.45, -0.4),
                                              group: CollisionGroups.ClickArea);
            }
        }

        protected override void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = Api.GetProtoEntity<SkeletonMechSkipper>();
        }
    }
}