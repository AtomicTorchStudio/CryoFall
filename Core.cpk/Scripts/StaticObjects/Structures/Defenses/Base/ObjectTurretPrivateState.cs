namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectTurretPrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
    {
        [SyncToClient]
        public IItemsContainer ContainerAmmo { get; set; }

        [SyncToClient]
        public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte PowerGridChargePercent { get; set; }

        public ICharacter ServerCharacterTurret { get; set; }

        [SyncToClient]
        public TurretMode TurretMode { get; set; }
    }
}