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

    public class VehicleHoverboardMk1 : ProtoVehicleHoverboard
    {
        public override string Description =>
            "Light variant of gravi-platform design. Relatively cheap to manufacture while offering great speed and fuel economy.";

        public override ushort EnergyUsePerSecondIdle => 10;

        public override ushort EnergyUsePerSecondMoving => 50;

        public override Color LightColor => LightColors.Flashlight.WithAlpha(0x88);

        public override Size2F LightLogicalSize => 10;

        public override Vector2D LightPositionOffset => (0, -0.25);

        public override Size2F LightSize => 4;

        public override string Name => "Hoverboard Mk1";

        public override double PhysicsBodyAccelerationCoef => 3;

        public override double PhysicsBodyFriction => 8;

        public override double StatMoveSpeed => 3.5;

        public override float StructurePointsMax => 300;

        public override TextureResource TextureResourceHoverboard { get; }
            = new("Vehicles/HoverboardMk1");

        public override TextureResource TextureResourceHoverboardLight { get; }
            = new("Vehicles/HoverboardMk1Light");

        public override double VehicleWorldHeight => 0.5;

        protected override SoundResource EngineSoundResource { get; }
            = new("Objects/Vehicles/Hoverboard/Engine1");

        protected override double EngineSoundVolume => 0.4;

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
                .Add<ItemStructuralPlating>(3)
                .Add<ItemImpulseEngine>(3)
                .Add<ItemComponentsElectronic>(10);

            repairStageRequiredItems
                .Add<ItemIngotSteel>(5);

            repairStagesCount = 5;
        }
    }
}