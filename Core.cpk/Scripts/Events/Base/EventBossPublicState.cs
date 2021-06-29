namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Physics;

    public class EventBossPublicState : EventWithAreaPublicState
    {
        [NonSerialized]
        public IPhysicsBody ServerBarrierPhysicsBody;
    }
}