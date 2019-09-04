namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    /// <summary>
    /// Base world object type. You cannot inherit from it directly.
    /// </summary>
    public abstract class ProtoWorldObject
        <TWorldObject,
         TPrivateState,
         TPublicState,
         TClientState>
        : ProtoGameObject<TWorldObject, TPrivateState, TPublicState, TClientState>,
          IProtoWorldObject<TWorldObject>,
          IProtoWorldObjectWithSoundPresets
        where TWorldObject : class, IWorldObject
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        // TODO: consider removing this
        public const string Notification_CannotInteractWhileDazed = "Cannot interact while dazed.";

        public bool IsInteractableObject { get; private set; }

        /// <summary>
        /// Gets sound material of object (used for damage/hit sounds).
        /// </summary>
        public abstract ObjectSoundMaterial ObjectSoundMaterial { get; }

        public ReadOnlySoundPreset<ObjectSound> SoundPresetObject { get; private set; }

        protected virtual ReadOnlySoundPreset<ObjectSoundMaterial> MaterialDestroySoundPreset
            => MaterialDestroySoundPresets.Default;

        public static void ClientOnCannotInteract(
            IWorldObject worldObject,
            string message,
            bool isOutOfRange = false)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                message,
                                                                isOutOfRange);
        }

        public static bool SharedHasObstaclesOnTheWay(
            ICharacter character,
            Vector2D characterCenter,
            IPhysicsSpace physicsSpace,
            IWorldObject worldObject,
            bool sendDebugEvents)
        {
            var worldObjectCenter = worldObject.TilePosition.ToVector2D() + worldObject.PhysicsBody.CenterOffset;
            var worldObjectPointClosestToCharacter = worldObject.PhysicsBody.ClampPointInside(
                characterCenter,
                CollisionGroups.Default,
                out var isSuccess);

            if (!isSuccess)
            {
                // the physics body seems to not have the default collider, let's check for the click area instead
                worldObjectPointClosestToCharacter = worldObject.PhysicsBody.ClampPointInside(
                    characterCenter,
                    CollisionGroups.ClickArea,
                    out _);
            }

            return SharedHasObstaclesOnTheWay(character,
                                              characterCenter,
                                              physicsSpace,
                                              worldObject,
                                              worldObjectCenter,
                                              worldObjectPointClosestToCharacter,
                                              sendDebugEvents);
        }

        public static bool SharedHasObstaclesOnTheWay(
            ICharacter character,
            Vector2D characterCenter,
            IPhysicsSpace physicsSpace,
            Vector2D worldObjectCenter,
            Vector2D worldObjectPointClosestToCharacter,
            bool sendDebugEvents)
        {
            return SharedHasObstaclesOnTheWay(character,
                                              characterCenter,
                                              physicsSpace,
                                              worldObject: null,
                                              worldObjectCenter,
                                              worldObjectPointClosestToCharacter,
                                              sendDebugEvents);
        }

        public void ClientInteractFinish(TWorldObject worldObject)
        {
            ValidateIsClient();
            if (ClientWorldObjectInteractHelper.CurrentlyInteractingWith == null)
            {
                // already finished interacting
                return;
            }

            if (ClientWorldObjectInteractHelper.CurrentlyInteractingWith != worldObject)
            {
                throw new Exception(
                    "Interacting with another object - " + ClientWorldObjectInteractHelper.CurrentlyInteractingWith);
            }

            ClientWorldObjectInteractHelper.CurrentlyInteractingWith = null;
            this.ClientInteractFinish(new ClientObjectData(worldObject));
        }

        /// <summary>
        /// Client interact with world object finish method. Called for current world object when Client release the left mouse
        /// button.
        /// </summary>
        /// <param name="worldObject">WorldObject of this object type.</param>
        public void ClientInteractFinish(IWorldObject worldObject)
        {
            if (!(worldObject is TWorldObject gameObject))
            {
                throw new ArgumentException("Wrong argument type", nameof(worldObject));
            }

            this.ClientInteractFinish(gameObject);
        }

        public void ClientInteractStart(TWorldObject worldObject)
        {
            ValidateIsClient();

            if (ClientWorldObjectInteractHelper.CurrentlyInteractingWith != null)
            {
                // already started interacting with some world object
                return;
            }

            var playerCharacter = Client.Characters.CurrentPlayerCharacter;
            if (!this.SharedCanInteract(playerCharacter, worldObject, writeToLog: true))
            {
                // cannot interact
                return;
            }

            ClientWorldObjectInteractHelper.CurrentlyInteractingWith = worldObject;
            this.ClientInteractStart(new ClientObjectData(worldObject));
        }

        /// <summary>
        /// Client interact with world object start method. Called for pointed world object when Client press the left mouse
        /// button.
        /// </summary>
        /// <param name="worldObject">WorldObject of this object type.</param>
        public void ClientInteractStart(IWorldObject worldObject)
        {
            if (!(worldObject is TWorldObject gameObject))
            {
                throw new ArgumentException("Wrong argument type", nameof(worldObject));
            }

            this.ClientInteractStart(gameObject);
        }

        public virtual void ClientOnServerPhysicsUpdate(
            IWorldObject worldObject,
            Vector2D serverPosition,
            bool forceReset)
        {
            Client.World.SetPosition(worldObject, serverPosition, forceReset);
        }

        public bool SharedCanInteract(ICharacter character, IWorldObject worldObject, bool writeToLog)
        {
            return this.SharedCanInteract(character, worldObject as TWorldObject, writeToLog);
        }

        public virtual bool SharedCanInteract(ICharacter character, TWorldObject worldObject, bool writeToLog)
        {
            if (!this.IsInteractableObject)
            {
                return false;
            }

            if (character.GetPublicState<ICharacterPublicState>().IsDead)
            {
                return false;
            }

            if (worldObject is IStaticWorldObject staticWorldObject)
            {
                if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                {
                    if (!PveSystem.SharedValidateInteractionIsNotForbidden(character,
                                                                           staticWorldObject,
                                                                           writeToLog))
                    {
                        // action forbidden by PvE system
                        return false;
                    }
                }
                else // PvP servers have newbie protection system
                {
                    if (!NewbieProtectionSystem.SharedValidateInteractionIsNotForbidden(character,
                                                                                        staticWorldObject,
                                                                                        writeToLog))
                    {
                        // action forbidden by newbie protection system
                        return false;
                    }
                }
            }

            return this.SharedIsInsideCharacterInteractionArea(character,
                                                               worldObject,
                                                               writeToLog);
        }

        public void SharedCreatePhysics(IWorldObject worldObject)
        {
            var physicsBody = worldObject.PhysicsBody;
            if (physicsBody == null)
            {
                return;
            }

            physicsBody.Reset();

            var createPhysicsData = new CreatePhysicsData((TWorldObject)worldObject, physicsBody);
            this.SharedCreatePhysics(createPhysicsData);
            this.SharedProcessCreatedPhysics(createPhysicsData);

            if (physicsBody.HasShapes)
            {
                physicsBody.IsEnabled = true;
            }
        }

        public abstract Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject);

        public bool SharedIsInsideCharacterInteractionArea(
            ICharacter character,
            IWorldObject worldObject,
            bool writeToLog,
            CollisionGroup requiredCollisionGroup = null)
        {
            return this.SharedIsInsideCharacterInteractionArea(
                character,
                worldObject as TWorldObject,
                writeToLog,
                requiredCollisionGroup);
        }

        /// <summary>
        /// Check if the character's interaction area collides with the world object click area.
        /// The character also should not be dead.
        /// </summary>
        public virtual bool SharedIsInsideCharacterInteractionArea(
            ICharacter character,
            TWorldObject worldObject,
            bool writeToLog,
            CollisionGroup requiredCollisionGroup = null)
        {
            if (worldObject.IsDestroyed)
            {
                return false;
            }

            try
            {
                this.VerifyGameObject(worldObject);
            }
            catch (Exception ex)
            {
                if (writeToLog)
                {
                    Logger.Exception(ex);
                }
                else
                {
                    Logger.Warning(ex.Message + " during " + nameof(SharedIsInsideCharacterInteractionArea));
                }

                return false;
            }

            if (character.GetPublicState<ICharacterPublicState>()
                         .IsDead)
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} - character is dead.",
                        character);
                }

                return false;
            }

            bool isInsideInteractionArea;
            if (worldObject.PhysicsBody.HasShapes)
            {
                // check that the world object is inside the interaction area of the character
                using var objectsInCharacterInteractionArea
                    = InteractionCheckerSystem.SharedGetTempObjectsInCharacterInteractionArea(
                        character,
                        writeToLog,
                        requiredCollisionGroup);
                isInsideInteractionArea =
                    objectsInCharacterInteractionArea?.Any(t => t.PhysicsBody.AssociatedWorldObject == worldObject)
                    ?? false;
            }
            else
            {
                // the world object doesn't have physics shapes
                // check this object tile by tile
                // ensure at least one tile of this object is inside the character interaction area
                // ensure there is direct line of sight between player character and this tile

                var characterInteractionAreaShape = character.PhysicsBody.Shapes.FirstOrDefault(
                    s => s.CollisionGroup
                         == CollisionGroups.CharacterInteractionArea);
                isInsideInteractionArea = false;
                foreach (var tileOffset in ((IProtoStaticWorldObject)worldObject.ProtoWorldObject).Layout.TileOffsets)
                {
                    var penetration = character.PhysicsBody.PhysicsSpace.TestShapeCollidesWithShape(
                        sourceShape: characterInteractionAreaShape,
                        targetShape: new RectangleShape(
                            position: (worldObject.TilePosition + tileOffset).ToVector2D()
                                      + (0.01, 0.01),
                            size: (0.98, 0.98),
                            collisionGroup: CollisionGroups.ClickArea),
                        sourceShapeOffset: character.PhysicsBody.Position);

                    if (!penetration.HasValue)
                    {
                        // this tile is not inside the character interaction area
                        continue;
                    }

                    // the tile is inside the character interaction area
                    // check that there is a direct line between the character and the tile
                    isInsideInteractionArea = true;
                    break;
                }
            }

            if (!isInsideInteractionArea)
            {
                // the world object is outside the character interaction area
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} - outside the interaction area.",
                        character);

                    if (IsClient)
                    {
                        ClientOnCannotInteract(worldObject, CoreStrings.Notification_TooFar, isOutOfRange: true);
                    }
                }

                return false;
            }

            if (character.ProtoCharacter is PlayerCharacterSpectator)
            {
                // don't test for obstacles for spectator character
                return true;
            }

            // check that there are no other objects on the way between them (defined by default layer)
            var physicsSpace = character.PhysicsBody.PhysicsSpace;
            var characterCenter = character.Position + character.PhysicsBody.CenterOffset;

            if (!SharedHasObstaclesOnTheWay(character,
                                            characterCenter,
                                            physicsSpace,
                                            worldObject,
                                            sendDebugEvents: writeToLog))
            {
                return true;
            }

            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - there are other objects on the way.",
                    character);

                if (IsClient)
                {
                    ClientOnCannotInteract(worldObject,
                                           CoreStrings.Notification_ObstaclesOnTheWay,
                                           isOutOfRange: true);
                }
            }

            return false;
        }

        bool IProtoWorldObject.SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            if (worldObject == null)
            {
                return true;
            }

            if (worldObject.ProtoWorldObject != this)
            {
                return worldObject.ProtoWorldObject.SharedIsAllowedObjectToInteractThrough(worldObject);
            }

            return this.SharedIsAllowedObjectToInteractThrough(worldObject);
        }

        protected virtual void ClientInteractFinish(ClientObjectData data)
        {
        }

        protected virtual void ClientInteractStart(ClientObjectData data)
        {
        }

        protected abstract void ClientOnObjectDestroyed(Vector2Ushort tilePosition);

        protected sealed override void PrepareProto()
        {
            base.PrepareProto();
            this.SoundPresetObject = this.PrepareSoundPresetObject();
            this.PrepareProtoWorldObject();

            var type = this.GetType();
            this.IsInteractableObject = type.HasOverride(nameof(ClientInteractStart),     isPublic: false)
                                        || type.HasOverride(nameof(ClientInteractFinish), isPublic: false);
        }

        protected virtual void PrepareProtoWorldObject()
        {
        }

        protected virtual ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectGeneric;
        }

        protected void ServerSendObjectDestroyedEvent(IWorldObject targetObject)
        {
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(targetObject, scopedBy);

            this.CallClient(
                scopedBy,
                _ => _.ClientRemote_OnObjectDestroyed(targetObject.TilePosition));
        }

        protected abstract void SharedCreatePhysics(CreatePhysicsData data);

        protected virtual bool SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            return false;
        }

        protected virtual void SharedProcessCreatedPhysics(CreatePhysicsData data)
        {
        }

        private static bool SharedHasObstaclesOnTheWay(
            ICharacter character,
            Vector2D characterCenter,
            IPhysicsSpace physicsSpace,
            [CanBeNull] IWorldObject worldObject,
            Vector2D worldObjectCenter,
            Vector2D worldObjectPointClosestToCharacter,
            bool sendDebugEvents)
        {
            // let's test by casting rays from character center to:
            // 0) world object center
            // 1) world object point closest to the character
            // 2) combined - take X from center, take Y from closest
            // 3) combined - take X from closest, take Y from center
            if (TestHasObstacle(worldObjectCenter)
                && TestHasObstacle(worldObjectPointClosestToCharacter)
                && TestHasObstacle((worldObjectCenter.X,
                                    worldObjectPointClosestToCharacter.Y))
                && TestHasObstacle((worldObjectPointClosestToCharacter.X, worldObjectCenter.Y)))
            {
                // has obstacle
                return true;
            }

            return false;

            // local method for testing if there is an obstacle from current to the specified position
            bool TestHasObstacle(Vector2D toPosition)
            {
                using var obstaclesOnTheWay = physicsSpace.TestLine(
                    characterCenter,
                    toPosition,
                    CollisionGroup.GetDefault(),
                    sendDebugEvent: sendDebugEvents);
                foreach (var test in obstaclesOnTheWay)
                {
                    var testPhysicsBody = test.PhysicsBody;
                    if (!(testPhysicsBody.AssociatedProtoTile is null))
                    {
                        // obstacle tile on the way
                        return true;
                    }

                    var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                    if (ReferenceEquals(testWorldObject, worldObject))
                    {
                        // not an obstacle - it's the world object itself
                        continue;
                    }

                    if (testWorldObject is ICharacter)
                    {
                        // characters are not assumed as an obstacle
                        continue;
                    }

                    // no need for this check anymore as we're checking for general "is ICharacter" above
                    //if (ReferenceEquals(testWorldObject, character))
                    //{
                    //    // not an obstacle - it's the player's character itself
                    //    continue;
                    //}

                    if (!testWorldObject.ProtoWorldObject
                                        .SharedIsAllowedObjectToInteractThrough(testWorldObject))
                    {
                        // obstacle object on the way
                        return true;
                    }
                }

                // no obstacles
                return false;
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OnObjectDestroyed(Vector2Ushort tilePosition)
        {
            this.ClientOnObjectDestroyed(tilePosition);
        }

        /// <summary>
        /// Data for ClientInteractStart() and ClientInteractFinish() methods.
        /// </summary>
        protected struct ClientObjectData
        {
            /// <summary>
            /// GameObject of this object type.
            /// </summary>
            public readonly TWorldObject GameObject;

            private TClientState clientState;

            private TPrivateState privateState;

            private TPublicState publicState;

            internal ClientObjectData(TWorldObject gameObject) : this()
            {
                this.GameObject = gameObject;
            }

            /// <summary>
            /// Client state for this world object.
            /// </summary>
            public TClientState ClientState => this.clientState ??= GetClientState(this.GameObject);

            /// <summary>
            /// Synchronized server private state for this world object.
            /// </summary>
            public TPrivateState PrivateState => this.privateState ??= GetPrivateState(this.GameObject);

            /// <summary>
            /// Synchronized server public state for this world object.
            /// </summary>
            public TPublicState PublicState => this.publicState ??= GetPublicState(this.GameObject);
        }

        protected struct CreatePhysicsData
        {
            public readonly TWorldObject GameObject;

            public readonly IPhysicsBody PhysicsBody;

            private TClientState clientState;

            private TPrivateState privateState;

            private TPublicState publicState;

            internal CreatePhysicsData(TWorldObject gameObject, IPhysicsBody physicsBody) : this()
            {
                this.GameObject = gameObject;
                this.PhysicsBody = physicsBody;
            }

            /// <summary>
            /// Client state for this world object.
            /// </summary>
            public TClientState ClientState => this.clientState ??= GetClientState(this.GameObject);

            /// <summary>
            /// Synchronized server private state for this world object.
            /// </summary>
            public TPrivateState PrivateState => this.privateState ??= GetPrivateState(this.GameObject);

            /// <summary>
            /// Synchronized server public state for this world object.
            /// </summary>
            public TPublicState PublicState => this.publicState ??= GetPublicState(this.GameObject);
        }
    }
}