namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDecorationTV
        : ProtoObjectDecoration<
            StructurePrivateState,
            ObjectDecorationTV.PublicState,
            ObjectDecorationTV.ClientState>
    {
        private const double ActiveDuration = 1 * 60; // TV will shut down after 1 minute when activated by player 

        /*private static readonly SoundResource SoundResourceActive
            = new("Objects/Structures/ObjectDecorationTV/Active");*/

        private static readonly SoundResource SoundResourceToggle
            = new("Objects/Structures/ObjectDecorationTV/Toggle");

        private readonly TextureAtlasResource textureAtlasActive;

        public ObjectDecorationTV()
        {
            this.textureAtlasActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Animation",
                columns: 8,
                rows: 2,
                isTransparent: true);
        }

        public override double ClientUpdateIntervalSeconds => 0.25;

        public override string Description =>
            "Centerpiece of a well-designed living room. Watch movies with your friends and relax.";

        public override string Name => "TV";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject) + (0, 0.6);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;
            var sceneObject = worldObject.ClientSceneObject;

            /*clientState.SoundEmitter = Client.Audio.CreateSoundEmitter(
                worldObject,
                SoundResourceActive,
                isLooped: true,
                volume: 0.5f,
                radius: 1.5f);
            clientState.SoundEmitter.CustomMaxDistance = 4f;*/

            // add sprite renderer
            var randomizer = (int)(PositionHashHelper.GetHashUInt32(worldObject.TilePosition.X,
                                                                    worldObject.TilePosition.Y)
                                   % 128);

            Vector2D offset = (52 / 256.0, 256 / 256.0);
            var overlaySpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                positionOffset: offset,
                spritePivotPoint: (0, 0),
                drawOrder: clientState.Renderer.DrawOrder);
            overlaySpriteRenderer.DrawOrderOffsetY = -offset.Y
                                                     - 0.01
                                                     + clientState.Renderer.DrawOrderOffsetY;

            // add sprite sheet animation for active TV image
            var componentAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimator.Setup(overlaySpriteRenderer,
                                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                                        this.textureAtlasActive),
                                    isLooped: true,
                                    frameDurationSeconds: 10 / 60.0,
                                    initialFrameOffset: randomizer);

            clientState.ComponentAnimator = componentAnimator;

            clientState.ComponentLightSource = ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (7, 9.5),
                positionOffset: (1, 1.4));

            publicState
                .ClientSubscribe(_ => _.ActiveUntil,
                                 _ =>
                                 {
                                     ClientRefreshActiveState(worldObject);
                                     Client.Audio.PlayOneShot(SoundResourceToggle, worldObject);
                                 },
                                 clientState);

            ClientRefreshActiveState(worldObject);
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            this.CallServer(_ => _.ServerRemote_ObjectUse(data.GameObject));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);
            ClientRefreshActiveState(data.GameObject);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlastic>(count: 1);
            build.AddStageRequiredItem<ItemWire>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlastic>(count: 1);
            repair.AddStageRequiredItem<ItemWire>(count: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((2.0, 0.7),  offset: (0.00, 0.0))
                .AddShapeRectangle((1.8, 1.0),  offset: (0.1, 0.7),   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.8, 1.0),  offset: (0.1, 0.8),   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((1.7, 1.85), offset: (0.15, 0.15), group: CollisionGroups.ClickArea);
        }

        private static void ClientRefreshActiveState(IStaticWorldObject worldObject)
        {
            var publicState = GetPublicState(worldObject);
            var clientState = GetClientState(worldObject);

            var isActive = Client.CurrentGame.ServerFrameTimeApproximated < publicState.ActiveUntil;
            clientState.ComponentAnimator.IsEnabled = isActive;
            //clientState.SoundEmitter.IsEnabled = isActive;
            clientState.ComponentLightSource.IsEnabled = isActive;
            clientState.ComponentAnimator.SpriteRenderer.IsEnabled = isActive;
        }

        [RemoteCallSettings(timeInterval: 0.5, keyArgIndex: 0, deliveryMode: DeliveryMode.ReliableSequenced)]
        private void ServerRemote_ObjectUse(IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoStaticWorldObject.SharedCanInteract(character,
                                                                      worldObject,
                                                                      writeToLog: true))
            {
                return;
            }

            var time = Server.Game.FrameTime;
            var publicState = GetPublicState(worldObject);

            var isActive = time < publicState.ActiveUntil;
            if (isActive)
            {
                // turn off
                publicState.ActiveUntil = 0;
                return;
            }

            // turn on
            publicState.ActiveUntil = time + ActiveDuration;
        }

        public class ClientState : StaticObjectClientState
        {
            public ClientComponentSpriteSheetAnimator ComponentAnimator { get; set; }

            public ClientComponentSpriteLightSource ComponentLightSource { get; set; }
        }

        public class PublicState : StaticObjectPublicState
        {
            [SyncToClient]
            [TempOnly]
            public double ActiveUntil { get; set; }
        }
    }
}