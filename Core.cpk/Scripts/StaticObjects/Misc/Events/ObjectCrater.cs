namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCrater : ProtoStaticWorldObject
    {
        public override StaticObjectKind Kind => StaticObjectKind.FloorDecal;

        [NotLocalizable]
        public override string Name => "Crater";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Glass;

        public override double ObstacleBlockDamageCoef => 0;

        public override float StructurePointsMax => 0; // non-damageable

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var clientState = data.ClientState;

            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.DefaultTexture,
                drawOrder: DrawOrder.Floor + 1);

            this.ClientSetupRenderer(clientState.Renderer);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Misc/Events/ObjectCrater",
                                       isTransparent: true);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // schedule destruction by timer
            var worldObject = data.GameObject;
            ServerTimersSystem.AddAction(
                delaySeconds: 60 * 60, // 60 minutes
                () => ServerDespawnTimerCallback(worldObject));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // no physics
        }

        private static void ServerDespawnTimerCallback(IStaticWorldObject worldObject)
        {
            if (!Server.World.IsObservedByAnyPlayer(worldObject))
            {
                // can destroy now
                Server.World.DestroyObject(worldObject);
                return;
            }

            // postpone destruction
            ServerTimersSystem.AddAction(
                delaySeconds: 60,
                () => ServerDespawnTimerCallback(worldObject));
        }
    }
}