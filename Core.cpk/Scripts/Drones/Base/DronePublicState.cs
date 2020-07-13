namespace AtomicTorch.CBND.CoreMod.Drones
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class DronePublicState : BasePublicState, IPublicStateWithStructurePoints
    {
        [SyncToClient]
        [TempOnly]
        public bool IsMining { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public float StructurePointsCurrent { get; set; }

        [SyncToClient]
        [TempOnly]
        public Vector2Ushort? TargetObjectPosition { get; private set; }

        [SyncToClient]
        [TempOnly]
        public Vector2D? TargetDronePosition { get; private set; }

        public void ResetTargetPosition()
        {
            this.TargetObjectPosition = null;
            this.TargetDronePosition = null;
            this.IsMining = false;
        }

        public void SetTargetPosition(Vector2Ushort targetObjectPosition, Vector2D targetDronePosition)
        {
            this.TargetObjectPosition = targetObjectPosition;
            this.TargetDronePosition = targetDronePosition;
            this.IsMining = false;
        }
    }
}