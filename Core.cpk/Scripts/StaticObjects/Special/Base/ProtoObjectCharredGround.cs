namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectCharredGround
        : ProtoStaticWorldObject
            <EmptyPrivateState, ObjectCharredGroundPublicState, StaticObjectClientState>
    {
        // 5 minutes postpone if object is observed by any player
        public const double ObjectDespawnDurationPostponeIfObservedSeconds = 5 * 60;

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public virtual bool IsRemovesOtherCharredGroundInOccupiedTiles => true;

        public override StaticObjectKind Kind => StaticObjectKind.FloorDecal;

        // despawn after 32 hours by default
        public virtual double ObjectDespawnDurationSeconds { get; } = 32 * 60 * 60;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0;

        public sealed override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 0; // non-damageable

        public static void ServerSetWorldOffset(IStaticWorldObject worldObject, Vector2F worldOffset)
        {
            var publicState = GetPublicState(worldObject);
            publicState.WorldOffset = worldOffset;
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
            if (renderer.SceneObject.AttachedWorldObject is not null)
            {
                var publicState = GetPublicState((IStaticWorldObject)renderer.SceneObject.AttachedWorldObject);
                worldOffset = publicState.WorldOffset.ToVector2D();
            }

            renderer.PositionOffset = worldOffset;
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.DrawOrder = DrawOrder.FloorCharredGround;
            renderer.Scale = 1.0f;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // schedule destruction by timer
            var worldObject = data.GameObject;
            ServerTimersSystem.AddAction(
                delaySeconds: PveSystem.ServerIsPvE
                                  // no sense in keeping the charred ground for too long on the PvE servers
                                  ? 30 * 60 // 30 minutes
                                  : this.ObjectDespawnDurationSeconds,
                () => ServerDespawnTimerCallback(worldObject));

            data.PublicState.WorldOffset = this.Layout.Center.ToVector2F();
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
                delaySeconds: ObjectDespawnDurationPostponeIfObservedSeconds,
                () => ServerDespawnTimerCallback(worldObject));
        }
    }
}