namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectVehicleAssemblyBay
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoVehicleAssemblyBay
        where TPrivateState : ObjectVehicleAssemblyBayPrivateState, new()
        where TPublicState : ObjectVehicleAssemblyBayPublicState, new()
        where TClientState : ObjectVehicleAssemblyBayClientState, new()
    {
        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public abstract Vector2D PlatformCenterWorldOffset { get; }

        /// <summary>
        /// Bounds to define an area which should not have obstacles when building a vehicle.
        /// </summary>
        protected abstract BoundsDouble BoundsNoObstaclesTest { get; }

        public BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            return WindowObjectVehicleAssemblyBay.Open((IStaticWorldObject)worldObject);
        }

        public void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        public void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public virtual void SharedGetVehiclesOnPlatform(
            IStaticWorldObject vehicleAssemblyBay,
            ITempList<IDynamicWorldObject> result)
        {
            var noObstaclesBounds = this.BoundsNoObstaclesTest;
            noObstaclesBounds = new BoundsDouble(
                offset: noObstaclesBounds.Offset + vehicleAssemblyBay.PhysicsBody.Position,
                size: noObstaclesBounds.Size);

            // test with different collision zones (required to handle hoverboards which don't have physical colliders)
            CollectVehicles(CollisionGroups.Default);

            if (!ReferenceEquals(CollisionGroups.Default,
                                 CollisionGroups.CharacterOrVehicle))
            {
                CollectVehicles(CollisionGroups.CharacterOrVehicle);
            }

            CollectVehicles(CollisionGroups.HitboxMelee);

            void CollectVehicles(CollisionGroup collisionGroup)
            {
                foreach (var testResult in vehicleAssemblyBay.PhysicsBody.PhysicsSpace.TestRectangle(
                    position: noObstaclesBounds.Offset,
                    size: noObstaclesBounds.Size,
                    collisionGroup: collisionGroup).EnumerateAndDispose())
                {
                    if (testResult.PhysicsBody.AssociatedWorldObject is IDynamicWorldObject dynamicWorldObject
                        && dynamicWorldObject.ProtoGameObject is IProtoVehicle)
                    {
                        result.AddIfNotContains(dynamicWorldObject);
                    }
                }
            }
        }

        public bool SharedIsBaySpaceBlocked(IStaticWorldObject vehicleAssemblyBay)
        {
            this.VerifyGameObject(vehicleAssemblyBay);
            return this.SharedIsBaySpaceBlockedInternal(vehicleAssemblyBay);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual bool SharedIsBaySpaceBlockedInternal(IStaticWorldObject vehicleAssemblyBay)
        {
            var noObstaclesBounds = this.BoundsNoObstaclesTest;
            noObstaclesBounds = new BoundsDouble(
                offset: noObstaclesBounds.Offset + vehicleAssemblyBay.PhysicsBody.Position,
                size: noObstaclesBounds.Size);

            // test with different collision zones (required to handle hoverboards which don't have physical colliders)
            var defaultCollisionGroup = CollisionGroups.Default;
            return HasObstacles(defaultCollisionGroup)
                   || HasObstacles(CollisionGroups.HitboxMelee);

            bool HasObstacles(CollisionGroup collisionGroup)
            {
                foreach (var testResult in vehicleAssemblyBay.PhysicsBody.PhysicsSpace.TestRectangle(
                    position: noObstaclesBounds.Offset,
                    size: noObstaclesBounds.Size,
                    collisionGroup: collisionGroup).EnumerateAndDispose())
                {
                    if (testResult.PhysicsBody.AssociatedWorldObject == vehicleAssemblyBay)
                    {
                        continue;
                    }

                    if (collisionGroup != defaultCollisionGroup
                        && testResult.PhysicsBody.HasAnyShapeCollidingWithGroup(defaultCollisionGroup))
                    {
                        // ignore this physics body as it has a physical collider
                        continue;
                    }

                    // space blocked
                    return true;
                }

                return false;
            }
        }
    }

    public abstract class ProtoObjectVehicleAssemblyBay
        : ProtoObjectVehicleAssemblyBay<
            ObjectVehicleAssemblyBayPrivateState,
            ObjectVehicleAssemblyBayPublicState,
            ObjectVehicleAssemblyBayClientState>
    {
    }
}