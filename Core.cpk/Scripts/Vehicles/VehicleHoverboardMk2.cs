namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class VehicleHoverboardMk2 : ProtoVehicleHoverboard
    {
        public override string Description =>
            "Heavy variant of gravi-platform design. More durable and offers higher speed and maneuverability at the cost of increased manufacturing expenses and fuel consumption.";

        public override ushort EnergyUsePerSecondIdle => 30;

        public override ushort EnergyUsePerSecondMoving => 120;

        public override Color LightColor => LightColors.Flashlight.WithAlpha(0x88);

        public override Size2F LightLogicalSize => 10;

        public override Vector2D LightPositionOffset => (0, -0.25);

        public override Size2F LightSize => 4;

        public override string Name => "Hoverboard Mk2";

        public override double PhysicsBodyAccelerationCoef => 4;

        public override double PhysicsBodyFriction => 8;

        public override double StatMoveSpeed => 4.0;

        public override float StructurePointsMax => 400;

        public override TextureResource TextureResourceHoverboard { get; }
            = new TextureResource("Vehicles/HoverboardMk2");

        public override TextureResource TextureResourceHoverboardLight { get; }
            = new TextureResource("Vehicles/HoverboardMk2Light");

        public override double VehicleWorldHeight => 0.5;

        protected override SoundResource EngineSoundResource { get; }
            = new SoundResource("Objects/Vehicles/Hoverboard/Process1");

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0, -0.3);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier2);
        }

        protected override void PrepareProtoVehicle(
            InputItems buildRequiredItems,
            InputItems repairStageRequiredItems,
            out int repairStagesCount)
        {
            buildRequiredItems
                .Add<ItemStructuralPlating>(5)
                .Add<ItemImpulseEngine>(5)
                .Add<ItemComponentsElectronic>(20)
                .Add<ItemComponentsOptical>(10);

            repairStageRequiredItems
                .Add<ItemIngotSteel>(5)
                .Add<ItemComponentsElectronic>(1)
                .Add<ItemComponentsOptical>(1);

            repairStagesCount = 5;
        }
    }
}