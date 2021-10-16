namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
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
        public virtual string InteractionTooltipText => InteractionTooltipTexts.Interact;

        public virtual bool IsInteractableObject { get; private set; }

        /// <summary>
        /// Gets sound material of object (used for damage/hit sounds).
        /// </summary>
        public abstract ObjectMaterial ObjectMaterial { get; }

        public ReadOnlySoundPreset<ObjectSound> SoundPresetObject { get; private set; }

        protected virtual ReadOnlySoundPreset<ObjectMaterial> MaterialDestroySoundPreset
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

        public virtual string ClientGetTitle(IWorldObject worldObject)
        {
            // only certain objects like structures are displaying the name tooltip
            return null;
        }

        public void ClientInteractFinish(TWorldObject worldObject)
        {
            ValidateIsClient();
            if (ClientWorldObjectInteractHelper.CurrentlyInteractingWith is null)
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
            if (worldObject is not TWorldObject gameObject)
            {
                throw new ArgumentException("Wrong argument type", nameof(worldObject));
            }

            this.ClientInteractFinish(gameObject);
        }

        public void ClientInteractStart(TWorldObject worldObject)
        {
            ValidateIsClient();

            if (ClientWorldObjectInteractHelper.CurrentlyInteractingWith is not null)
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
            if (worldObject is not TWorldObject gameObject)
            {
                throw new ArgumentException("Wrong argument type", nameof(worldObject));
            }

            this.ClientInteractStart(gameObject);
        }

        public void ClientObserving(IWorldObject worldObject, bool isObserving)
        {
            this.ClientObserving(new ClientObjectData((TWorldObject)worldObject), isObserving);
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

            try
            {
                this.VerifyGameObject(worldObject);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Interaction check failed: {ex.GetType().FullName}: {ex.Message}");
                return false;
            }

            if (character.GetPublicState<ICharacterPublicState>().IsDead
                || IsServer && !character.ServerIsOnline
                || ((IsServer || character.IsCurrentClientCharacter)
                    && PlayerCharacter.GetPrivateState(character).IsDespawned))
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} - character is offline/despawned/dead.",
                        character);
                }

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
                    if (!NewbieProtectionSystem.SharedValidateInteractionIsNotForbidden(
                            character,
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
            if (physicsBody is null)
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

            if (character.GetPublicState<ICharacterPublicState>().IsDead
                || IsServer && !character.ServerIsOnline
                || ((IsServer || character.IsCurrentClientCharacter)
                    && PlayerCharacter.GetPrivateState(character).IsDespawned))
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} - character is offline/despawned/dead.",
                        character);
                }

                return false;
            }

            bool isInsideInteractionArea;
            if (worldObject.PhysicsBody.HasShapes
                && worldObject.PhysicsBody.HasAnyShapeCollidingWithGroup(CollisionGroups.ClickArea))
            {
                // check that the world object is inside the interaction area of the character
                using var objectsInCharacterInteractionArea
                    = InteractionCheckerSystem.SharedGetTempObjectsInCharacterInteractionArea(
                        character,
                        writeToLog,
                        requiredCollisionGroup);

                isInsideInteractionArea = false;
                if (objectsInCharacterInteractionArea is not null)
                {
                    foreach (var t in objectsInCharacterInteractionArea.AsList())
                    {
                        if (!ReferenceEquals(worldObject, t.PhysicsBody.AssociatedWorldObject))
                        {
                            continue;
                        }

                        isInsideInteractionArea = true;
                        break;
                    }
                }
            }
            else if (worldObject.ProtoWorldObject is IProtoStaticWorldObject protoStaticWorldObject)
            {
                // the world object doesn't have click area collision shapes
                // check this object tile by tile
                // ensure at least one tile of this object is inside the character interaction area
                // ensure there is direct line of sight between player character and this tile

                var characterInteractionAreaShape = character.PhysicsBody.Shapes.FirstOrDefault(
                    s => s.CollisionGroup
                         == CollisionGroups.CharacterInteractionArea);
                isInsideInteractionArea = false;
                foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
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
            else
            {
                isInsideInteractionArea = false;
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

            if (!ObstacleTestHelper.SharedHasObstaclesInTheWay(characterCenter,
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
            if (worldObject is null)
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

        protected virtual void ClientObserving(ClientObjectData data, bool isObserving)
        {
        }

        protected abstract void ClientOnObjectDestroyed(Vector2D position);

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

            var position = targetObject is IDynamicWorldObject dynamicWorldObject
                               ? dynamicWorldObject.Position
                               : targetObject.TilePosition.ToVector2D();

            this.CallClient(
                scopedBy.AsList(),
                _ => _.ClientRemote_OnObjectDestroyed(position));
        }

        protected abstract void SharedCreatePhysics(CreatePhysicsData data);

        protected virtual bool SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            return false;
        }

        protected virtual void SharedProcessCreatedPhysics(CreatePhysicsData data)
        {
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OnObjectDestroyed(Vector2D position)
        {
            this.ClientOnObjectDestroyed(position);
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