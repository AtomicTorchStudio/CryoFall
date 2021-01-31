namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Explosives;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemExplosiveActionPublicState : BasePublicActionState
    {
        private IClientSceneObject sceneObjectVisualizer;

        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        public IProtoItemExplosive ProtoItemExplosive { get; set; }

        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        [TempOnly]
        public Vector2Ushort TargetPosition { get; set; }

        protected override void ClientDeinitialize()
        {
            this.sceneObjectVisualizer?.Destroy();
            this.sceneObjectVisualizer = null;
        }

        protected override void ClientOnCompleted()
        {
            if (this.IsCancelled)
            {
                return;
            }

            this.ProtoItemExplosive?.SharedGetItemSoundPreset()
                .PlaySound(ItemSound.Use, this.Character);
        }

        protected override void ClientOnStart()
        {
            if (this.Character.IsCurrentClientCharacter)
            {
                return;
            }

            this.sceneObjectVisualizer = Api.Client.Scene.CreateSceneObject(
                "Temp explosive placement visualizer",
                this.TargetPosition.ToVector2D());

            var tile = Api.Client.World.GetTile(this.TargetPosition);
            this.ProtoItemExplosive.ObjectExplosiveProto.ClientSetupBlueprint(
                tile,
                new ClientBlueprintRenderer(this.sceneObjectVisualizer, isConstructionSite: false));
        }
    }
}