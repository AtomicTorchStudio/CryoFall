namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EventBossClientState : BaseClientState
    {
        /// <summary>
        /// Barrier (a circle impenetrable area) is present only in PvE to prevent players from rushing
        /// into the boss area before the boss is spawned.
        /// </summary>
        public IPhysicsBody BarrierPhysicsBody;

        public IClientSceneObject SceneObjectBarrierVisualizer;
    }
}