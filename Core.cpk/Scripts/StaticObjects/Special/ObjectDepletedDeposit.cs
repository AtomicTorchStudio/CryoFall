namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDepletedDeposit
        : ProtoStaticWorldObject
            <EmptyPrivateState,
                StaticObjectPublicState,
                StaticObjectClientState>
    {
        // 5 minutes postpone if object is observed by any player or when there is an extractor built on top of it
        public const double ObjectDespawnDurationPostponeIfObservedSeconds = 5 * 60;

        // 24 hours (setting it higher might lead to an issue where there is no space to spawn on a very populated server)
        public const double ObjectDespawnDurationSeconds = 24 * 60 * 60;

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override bool HasIncreasedScopeSize => true;

        // we want to ensure the spawn conditions will use this as a restriction
        public override bool IsIgnoredBySpawnScripts => false;

        // players cannot remove it, it will disappear on its own after some time
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        [NotLocalizable]
        public override string Name => "Depleted deposit";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0;

        public sealed override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 0; // non-damageable

        public static IStaticWorldObject SharedGetDepletedDepositWorldObject(Vector2Ushort tilePosition)
        {
            var tile = IsServer
                           ? Server.World.GetTile(tilePosition)
                           : Client.World.GetTile(tilePosition);

            return SharedGetDepletedDepositWorldObject(tile);
        }

        public static IStaticWorldObject SharedGetDepletedDepositWorldObject(Tile tile)
        {
            foreach (var obj in tile.StaticObjects)
            {
                if (obj.ProtoStaticWorldObject is ObjectDepletedDeposit)
                {
                    return obj;
                }
            }

            return null;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false;      // no hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var tilePosition = data.GameObject.TilePosition;
            var renderer = data.ClientState.Renderer;
            if (ClientGroundExplosionAnimationHelper.IsGroundSpriteFlipped(tilePosition))
            {
                renderer.DrawMode = DrawMode.FlipHorizontally;
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            var worldOffset = this.Layout.Center;
            renderer.PositionOffset = worldOffset;
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.DrawOrder = DrawOrder.FloorCharredGround;
            renderer.Scale = 1.0f;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Special/ObjectDepletedDeposit",
                                       isTransparent: true);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // schedule destruction by timer
            var worldObject = data.GameObject;
            ServerTimersSystem.AddAction(
                delaySeconds: ObjectDespawnDurationSeconds,
                () => ServerDespawnTimerCallback(worldObject));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // no physics
        }

        private static void ServerDespawnTimerCallback(IStaticWorldObject worldObject)
        {
            // can destroy only if there is no players observing
            // and no extractor built on top
            if (!Server.World.IsObservedByAnyPlayer(worldObject)
                && !worldObject.OccupiedTile.StaticObjects.Any(
                    s => s.ProtoStaticWorldObject is IProtoObjectExtractor))
            {
                // can destroy now
                Server.World.DestroyObject(worldObject);
                return;
            }

            // postpone destruction
            ServerTimersSystem.AddAction(
                delaySeconds: ObjectDespawnDurationPostponeIfObservedSeconds,
                () => ServerDespawnTimerCallback(worldObject));
        }
    }
}