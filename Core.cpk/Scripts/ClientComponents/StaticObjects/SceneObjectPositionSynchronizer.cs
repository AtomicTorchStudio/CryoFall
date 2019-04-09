namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class SceneObjectPositionSynchronizer : ClientComponent
    {
        private IClientSceneObject sceneObjectToSyncWith;

        public SceneObjectPositionSynchronizer() : base(isLateUpdateEnabled: true)
        {
        }

        public override void LateUpdate(double deltaTime)
        {
            this.SyncPosition();
        }

        public void Setup(IClientSceneObject sceneObjectToSyncWith)
        {
            this.sceneObjectToSyncWith = sceneObjectToSyncWith;
        }

        public override void Update(double deltaTime)
        {
            this.SyncPosition();
        }

        private void SyncPosition()
        {
            if (this.sceneObjectToSyncWith.IsDestroyed)
            {
                this.SceneObject.Destroy();
                return;
            }

            this.SceneObject.Position = this.sceneObjectToSyncWith.Position;
        }
    }
}