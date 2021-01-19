namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectTurretPublicState : StaticObjectPublicState, IObjectElectricityConsumerPublicState
    {
        [SyncToClient]
        public ElectricityConsumerState ElectricityConsumerState { get; set; }

        /// <summary>
        /// An indicator flag when the turret requires ammo but have no loaded ammo.
        /// </summary>
        [SyncToClient]
        [TempOnly]
        public bool HasNoAmmo { get; set; }
    }
}