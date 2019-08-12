namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMineralPragmiumNode
        : ProtoObjectMineral
            <ObjectMineralPragmiumNode.PrivateState, StaticObjectPublicState, DefaultMineralClientState>
    {
        // The Node destruction will be postponed on this duration
        // if it cannot be destroy because there are characters observing it.
        private static readonly double DestructionTimeoutPostponeSeconds
            = TimeSpan.FromMinutes(2).TotalSeconds;

        // The Node will destroy after this duration if there is no Pragmium Source nearby
        // and there are no characters observing it.
        // Please note: the value is carefully selected to match with SpawnPragmiumWasteland respawn rate.
        private static readonly double DestructionTimeoutSeconds
            = TimeSpan.FromMinutes(29).TotalSeconds;

        private TextureResource[] textures;

        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Small pragmium node";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ServerUpdateIntervalSeconds => 60;

        public override float StructurePointsMax => 1000;

        protected override ITextureResource ClientGetTextureResource(
            IStaticWorldObject gameObject,
            StaticObjectPublicState publicState)
        {
            return this.textures[PositionalRandom.Get(gameObject.TilePosition,
                                                      minInclusive: 0,
                                                      maxExclusive: this.textures.Length,
                                                      seed: 791838756)];
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ObjectMineralPragmiumHelper.ClientInitializeLightForNode(data.GameObject);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.PositionOffset = (0.5, 0.5);
            renderer.DrawOrderOffsetY = -0.15;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            using var tempFiles = Api.Shared.FindFiles(ContentPaths.Textures + GenerateTexturePath(thisType));
            this.textures = new TextureResource[tempFiles.Count];

            var list = tempFiles.AsList();
            for (var index = 0; index < list.Count; index++)
            {
                var tempFile = list[index];
                this.textures[index] = new TextureResource(tempFile);
            }

            return this.textures[0];
        }

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            config.Stage4.Add<ItemOrePragmium>(count: 2);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                ServerRestartDestroyTimer(data.PrivateState);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var privateState = data.PrivateState;
            var timeNow = Server.Game.FrameTime;

            // Destroy Pragmium node if the timeout is exceeded
            // and there is no Pragmium Source node nearby
            // and there are no player characters observing it.
            if (timeNow < privateState.DestroyAtTime)
            {
                return;
            }

            // should destroy because timed out
            var worldObject = data.GameObject;
            if (Server.World.IsObservedByAnyPlayer(worldObject))
            {
                // cannot destroy - there are players observing it
                privateState.DestroyAtTime = timeNow + DestructionTimeoutPostponeSeconds;
                return;
            }

            if (worldObject.OccupiedTile.EightNeighborTiles.Any(
                tile => tile.StaticObjects.Any(
                    o => o.ProtoStaticWorldObject is ObjectMineralPragmiumSource)))
            {
                // cannot destroy - there is a Pragmium Source nearby
                ServerRestartDestroyTimer(privateState);
                return;
            }

            Server.World.DestroyObject(worldObject);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.2,  center: (0.5, 0.5))
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.HitboxMelee);
            // no ranged hitbox
        }

        private static void ServerRestartDestroyTimer(PrivateState privateState)
        {
            privateState.DestroyAtTime = Server.Game.FrameTime + DestructionTimeoutSeconds;
        }

        public class PrivateState : BasePrivateState
        {
            public double DestroyAtTime { get; set; }
        }
    }
}