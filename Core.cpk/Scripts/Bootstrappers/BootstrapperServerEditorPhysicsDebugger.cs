namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Helpers.Physics;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
    public class BootstrapperServerEditorPhysicsDebugger : BaseBootstrapper
    {
        private IPhysicsSpace currentPhysicsSpace;

        private IPhysicsSpace CurrentPhysicsSpace
        {
            get => this.currentPhysicsSpace;
            set
            {
                if (this.currentPhysicsSpace == value)
                {
                    return;
                }

                if (this.currentPhysicsSpace != null)
                {
                    this.currentPhysicsSpace.DebugShapeTesting -= PhysicsSpaceDebugTestingHandler;
                }

                this.currentPhysicsSpace = value;
                this.currentPhysicsSpace.DebugShapeTesting += PhysicsSpaceDebugTestingHandler;
            }
        }

        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            if (!Api.IsEditor)
            {
                return;
            }

            Server.World.WorldBoundsChanged += this.WorldBoundsChangedHandler;

            this.SetCurrentPhysicsSpace();
        }

        private static void PhysicsSpaceDebugTestingHandler(IPhysicsShape shape)
        {
            SharedEditorPhysicsDebugger.ServerSendDebugPhysicsTesting(shape);
        }

        private void SetCurrentPhysicsSpace()
        {
            this.CurrentPhysicsSpace = Server.World.GetPhysicsSpace();
        }

        private void WorldBoundsChangedHandler()
        {
            this.SetCurrentPhysicsSpace();
        }
    }
}