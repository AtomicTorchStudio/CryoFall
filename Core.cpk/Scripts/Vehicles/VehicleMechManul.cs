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

    // TODO: add this vehicle into the game
    public class VehicleMechManul : ProtoVehicleMech
    {
        public override byte CargoItemsSlotsCount => 24;

        public override string Description =>
            "Medium design for mechanized battle armor. Boasts substantial defensive capabilities and massive cargo holds without sacrificing in other areas.";

        public override ushort EnergyUsePerSecondIdle => 100;

        public override ushort EnergyUsePerSecondMoving => 300;

        public override BaseItemsContainerMechEquipment EquipmentItemsContainerType
            => Api.GetProtoEntity<ContainerMechEquipmentSkipper>();

        public override bool IsPlayersHotbarAndEquipmentItemsAllowed => false;

        public override double MaxDistanceToInteract => 1;

        public override string Name => "Manul";

        public override double PhysicsBodyAccelerationCoef => 3;

        public override double PhysicsBodyFriction => 10;

        public override double StatMoveSpeed => 2.0;

        public override double StatMoveSpeedRunMultiplier => 1.0; // no run mode

        public override float StructurePointsMax => 600;

        public override double VehicleWorldHeight => 2.0;

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void PrepareProtoVehicle(
            InputItems buildRequiredItems,
            InputItems repairStageRequiredItems,
            out int repairStagesCount)
        {
            // TODO
            buildRequiredItems.Add<ItemIngotSteel>(100)
                              .Add<ItemComponentsElectronic>(10)
                              .Add<ItemComponentsOptical>(10)
                              .Add<ItemComponentsHighTech>(10);

            repairStageRequiredItems.Add<ItemIngotSteel>(10)
                                    .Add<ItemComponentsElectronic>(1)
                                    .Add<ItemComponentsOptical>(1)
                                    .Add<ItemComponentsHighTech>(1);

            repairStagesCount = 5;
        }

        protected override void PrepareProtoVehicleLightConfig(ItemLightConfig lightConfig)
        {
            lightConfig.Color = LightColors.Flashlight;
            lightConfig.ScreenOffset = (3, -1);
            lightConfig.Size = 18;
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
                                              @group: CollisionGroups.ClickArea);
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