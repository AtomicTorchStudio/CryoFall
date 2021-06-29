namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleNamesSystem;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoVehicle
        <TVehiclePrivateState,
         TVehiclePublicState,
         TVehicleClientState>
        : ProtoDynamicWorldObject
          <TVehiclePrivateState,
              TVehiclePublicState,
              TVehicleClientState>,
          IProtoVehicle
        where TVehiclePrivateState : VehiclePrivateState, new()
        where TVehiclePublicState : VehiclePublicState, new()
        where TVehicleClientState : VehicleClientState, new()
    {
        // cannot build a vehicle
        public const string Notification_CannotBuildVehicleTitle = "Cannot build";

        // cannot repair a vehicle
        public const string Notification_CannotRepairVehicleTitle = "Cannot repair";

        // after 60 seconds in offline player will be removed from the vehicle
        private const double ThresholdDurationRemoveOfflinePlayerFromVehicle = 60;

        // how long the items dropped on the ground from the destroyed vehicle should remain there
        protected static readonly TimeSpan DestroyedCargoDroppedItemsDestructionTimeout = TimeSpan.FromHours(1);

        private List<TechNode> listedInTechNodes;

        protected ProtoVehicle()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("Vehicle", StringComparison.Ordinal))
            {
                throw new Exception("Vehicle class name must start with \"Vehicle\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("Vehicle".Length);

            // ReSharper disable once VirtualMemberCallInConstructor
            this.Icon = IsClient
                            ? this.ClientCreateIcon()
                            : TextureResource.NoTexture;
        }

        public virtual uint BuildRequiredElectricityAmount => 5000;

        public IReadOnlyList<ProtoItemWithCount> BuildRequiredItems { get; private set; }

        public bool CanChangeFactionRoleAccessForSelfRole => false; // don't allow to cut access to self

        public abstract byte CargoItemsSlotsCount { get; }

        public override double ClientUpdateIntervalSeconds => 0; // every frame

        public abstract string Description { get; }

        public virtual float DestroyedExplosionVolume => 1;

        public DamageDescription DestroyedVehicleDamageDescriptionCharacters { get; private set; }

        public double DestroyedVehicleDamageRadius { get; private set; }

        public abstract ushort EnergyUsePerSecondIdle { get; }

        public abstract ushort EnergyUsePerSecondMoving { get; }

        public virtual byte FuelItemsSlotsCount => 3;

        public bool HasOwnersList => true;

        public bool HasVehicleLights => this.VehicleLightConfig.IsLightEnabled;

        public ITextureResource Icon { get; }

        public abstract bool IsAllowCreatureDamageWhenNoPilot { get; }

        public virtual bool IsArmorBarDisplayedWhenPiloted { get; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public virtual bool IsAutoUnlocked => false;

        public bool IsClosedAccessModeAvailable => false;

        public bool IsEveryoneAccessModeAvailable => false;

        public abstract bool IsHeavyVehicle { get; }

        public abstract bool IsPlayersHotbarAndEquipmentItemsAllowed { get; }

        public IReadOnlyList<TechNode> ListedInTechNodes
            => this.listedInTechNodes
               ?? (IReadOnlyList<TechNode>)Array.Empty<TechNode>();

        public abstract ITextureResource MapIcon { get; }

        public abstract double MaxDistanceToInteract { get; }

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public abstract double PhysicsBodyAccelerationCoef { get; }

        public abstract double PhysicsBodyFriction { get; }

        public virtual uint RepairRequiredElectricityAmount => 500;

        public IReadOnlyList<ProtoItemWithCount> RepairStageRequiredItems { get; private set; }

        public int RepairStagesCount { get; private set; }

        public override double ServerUpdateIntervalSeconds => 0; // every frame

        public override double ServerUpdateRareIntervalSeconds => 10; // once in 10 seconds

        public override string ShortId { get; }

        public virtual SoundResource SoundResourceLightsToggle { get; }

        public virtual float SoundResourceLightsToggleVolume => 0.5f;

        public abstract SoundResource SoundResourceVehicleDismount { get; }

        public abstract SoundResource SoundResourceVehicleMount { get; }

        public abstract double StatMoveSpeed { get; }

        public abstract double StatMoveSpeedRunMultiplier { get; }

        public ItemLightConfig VehicleLightConfig { get; private set; }

        public abstract double VehicleWorldHeight { get; }

        protected virtual IProtoItemsContainer CargoItemsContainerType
            => Api.GetProtoEntity<ItemsContainerDefault>();

        protected virtual byte DestroyedExplosionNetworkRadius => 30;

        protected ExplosionPreset DestroyedExplosionPreset { get; private set; }

        protected IReadOnlyList<Vector2D> DismountPoints { get; private set; }

        protected virtual IProtoItemsContainer FuelItemsContainerType
            => Api.GetProtoEntity<ItemsContainerFuelCores>();

        public sealed override void ClientDeinitialize(IDynamicWorldObject gameObject)
        {
            this.ClientTryDestroyCurrentPlayerUI(gameObject);
            base.ClientDeinitialize(gameObject);
            this.ClientDeinitializeVehicle(gameObject);
        }

        public override string ClientGetTitle(IWorldObject worldObject)
        {
            return VehicleNamesSystem.ClientTryGetVehicleName(worldObject.Id)
                   ?? this.Name;
        }

        public override void ClientOnServerPhysicsUpdate(
            IWorldObject worldObject,
            Vector2D serverPosition,
            bool forceReset)
        {
            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            var currentCharacterPublicState = PlayerCharacter.GetPublicState(currentCharacter);

            if (!ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled
                || worldObject != currentCharacterPublicState.CurrentVehicle)
            {
                base.ClientOnServerPhysicsUpdate(worldObject, serverPosition, forceReset);
                return;
            }

            // current character - use prediction system
            if (currentCharacterPublicState.IsDead)
            {
                // don't use prediction system - player is dead
                // don't change the position (don't move the corpse!)
                //base.ClientOnServerPhysicsUpdate(worldObject, serverPosition, forceReset);
                return;
            }

            ClientCurrentCharacterLagPredictionManager.UpdatePosition(forceReset, (IDynamicWorldObject)worldObject);
        }

        public virtual void ClientOnVehicleDismounted(IDynamicWorldObject vehicle)
        {
        }

        public virtual BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            return WindowObjectVehicle.Open((IDynamicWorldObject)worldObject);
        }

        public async void ClientRequestBuild()
        {
            var result = this.SharedPlayerCanBuild(ClientCurrentCharacterHelper.Character);
            if (result != VehicleCanBuildCheckResult.Success)
            {
                this.ClientShowNotificationCannotBuild(result);
                return;
            }

            result = await this.CallServer(_ => _.ServerRemote_BuildVehicle());
            if (result != VehicleCanBuildCheckResult.Success)
            {
                this.ClientShowNotificationCannotBuild(result);
                return;
            }

            WindowObjectVehicleAssemblyBay.CloseActiveMenu();
            Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleBuilt);
        }

        public async void ClientRequestRepair()
        {
            var result = this.SharedPlayerCanRepairInVehicleAssemblyBay(ClientCurrentCharacterHelper.Character);
            if (result != VehicleCanRepairCheckResult.Success)
            {
                this.ClientShowNotificationCannotRepair(result);
                return;
            }

            result = await this.CallServer(_ => _.ServerRemote_RepairVehicle());
            if (result != VehicleCanRepairCheckResult.Success)
            {
                this.ClientShowNotificationCannotRepair(result);
                return;
            }

            Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleRepair);
        }

        public virtual void ClientSetupSkeleton(
            IDynamicWorldObject vehicle,
            IProtoCharacterSkeleton protoSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            if (!this.HasVehicleLights)
            {
                return;
            }

            var publicState = GetPublicState(vehicle);
            var componentLightSource = this.ClientCreateLightSource(vehicle);
            var componentLightInSkeleton = vehicle.ClientSceneObject.AddComponent<ClientComponentLightInSkeleton>();
            componentLightInSkeleton.Setup(skeletonRenderer,
                                           this.VehicleLightConfig,
                                           componentLightSource,
                                           skeletonSlotName: "Torso",
                                           isPrimaryLight: true);

            skeletonComponents.Add(componentLightInSkeleton);
            skeletonComponents.Add(componentLightSource);

            RefreshLights();

            publicState.ClientSubscribe(
                _ => _.IsLightsEnabled,
                _ =>
                {
                    RefreshLights();

                    var currentPilot = publicState.PilotCharacter;
                    if (currentPilot is not null
                        && !currentPilot.IsCurrentClientCharacter)
                    {
                        Api.Client.Audio.PlayOneShot(this.SoundResourceLightsToggle,
                                                     vehicle,
                                                     volume: this.SoundResourceLightsToggleVolume);
                    }
                },
                subscriptionOwner: GetClientState(vehicle));

            void RefreshLights()
            {
                if (!skeletonRenderer.IsEnabled)
                {
                    return;
                }

                var isLightsEnabled = publicState.IsLightsEnabled;
                componentLightSource.IsEnabled = componentLightInSkeleton.IsEnabled = isLightsEnabled;
                skeletonRenderer.SetAttachment("Lights",
                                               isLightsEnabled ? "LightsOn" : null,
                                               forSkeleton: protoSkeleton.SkeletonResourceFront);
            }
        }

        public void ClientToggleLight()
        {
            if (!this.HasVehicleLights)
            {
                return;
            }

            var character = ClientCurrentCharacterHelper.Character;
            var vehicle = character.SharedGetCurrentVehicle();
            var publicState = GetPublicState(vehicle);
            var setIsActive = !publicState.IsLightsEnabled;
            this.ClientTrySetLightsActiveState(vehicle, setIsActive);
        }

        public void ClientTrySetLightsActiveState(IDynamicWorldObject vehicle, bool setIsActive)
        {
            if (!this.HasVehicleLights)
            {
                return;
            }

            this.VerifyGameObject(vehicle);

            var character = ClientCurrentCharacterHelper.Character;
            var publicState = GetPublicState(vehicle);
            if (publicState.PilotCharacter != character)
            {
                Logger.Warning(
                    $"Only pilot can toggle the vehicles light: {vehicle} player is not the pilot: {character}");
                return;
            }

            if (publicState.IsLightsEnabled == setIsActive)
            {
                return;
            }

            // cannot change here because we need to wait for server callback on it unlike item light
            //publicState.IsLightsEnabled = setIsActive;
            Logger.Info($"Player switching vehicle light mode: {vehicle}, isActive={setIsActive}");
            this.CallServer(_ => _.ServerRemote_SetCurrentVehicleLightsMode(setIsActive));

            Api.Client.Audio.PlayOneShot(this.SoundResourceLightsToggle,
                                         vehicle,
                                         volume: this.SoundResourceLightsToggleVolume);
        }

        public void PrepareProtoSetLinkWithTechNode(TechNode techNode)
        {
            if (this.listedInTechNodes is null)
            {
                if (this.IsAutoUnlocked)
                {
                    Logger.Error(
                        this
                        + " is marked as "
                        + nameof(this.IsAutoUnlocked)
                        + " but the technology is set as the prerequisite: "
                        + techNode);
                }

                this.listedInTechNodes = new List<TechNode>();
            }

            this.listedInTechNodes.AddIfNotContains(techNode);
        }

        public void ServerOnBuilt(IDynamicWorldObject worldObject, ICharacter byCharacter)
        {
            WorldObjectOwnersSystem.ServerOnBuilt(worldObject, byCharacter);
        }

        public virtual void ServerOnCharacterEnterVehicle(IDynamicWorldObject vehicle, ICharacter character)
        {
            if (this.HasVehicleLights)
            {
                GetPublicState(vehicle).IsLightsEnabled = true;
            }
        }

        public virtual void ServerOnCharacterExitVehicle(IDynamicWorldObject vehicle, ICharacter character)
        {
            if (this.HasVehicleLights)
            {
                GetPublicState(vehicle).IsLightsEnabled = false;
            }

            if (this.DismountPoints.Count == 0)
            {
                return;
            }

            // try to find a free dismount location to exit the player character
            var physicsSpace = Server.World.GetPhysicsSpace();
            var collisionGroup = CollisionGroups.Default;
            var vehiclePosition = vehicle.Position;
            const double radius = ProtoCharacterSkeletonHuman.LegsColliderRadius;
            var testRectangleOffset = (-radius, -radius / 2);
            foreach (var offset in this.DismountPoints)
            {
                var dismountPosition = vehiclePosition + offset;

                var testResults = physicsSpace.TestRectangle(dismountPosition + testRectangleOffset,
                                                             size: (2 * radius, radius),
                                                             collisionGroup: collisionGroup);
                if (testResults.Count == 0)
                {
                    // empty location found
                    Server.World.SetPosition(character, dismountPosition);
                    return;
                }
            }
        }

        public void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
            var privateState = GetPrivateState((IDynamicWorldObject)worldObject);
            privateState.ServerTimeSinceLastUse = 0;
            this.ServerSetUpdateRate(worldObject, isRare: false);
        }

        public override void ServerOnDestroy(IDynamicWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var publicState = GetPublicState(gameObject);
            var privateState = GetPrivateState(gameObject);
            var pilotCharacter = publicState.PilotCharacter
                                 ?? privateState.ServerLastPilotCharacter;

            VehicleSystem.ServerOnVehicleDestroyed(gameObject);

            // try drop cargo on the ground
            var itemsContainer = privateState.CargoItemsContainer;
            var objectGroundContainer = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.Tile,
                itemsContainer,
                DestroyedCargoDroppedItemsDestructionTimeout.TotalSeconds);

            ServerTryClaimGroundContainerWithDroppedGoods(objectGroundContainer, pilotCharacter, privateState);
        }

        public void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public virtual void ServerOnPilotDamage(
            WeaponFinalCache weaponCache,
            IDynamicWorldObject vehicle,
            ICharacter pilotCharacter,
            double damageApplied)
        {
        }

        public void ServerOnRepair(IDynamicWorldObject vehicle, ICharacter character)
        {
            var structurePointsMax = this.SharedGetStructurePointsMax(vehicle);
            var structurePointsRestorePerRepairStage = structurePointsMax / this.RepairStagesCount;

            var publicState = GetPublicState(vehicle);
            var previousStructurePoints = publicState.StructurePointsCurrent;
            var newStructurePoints = previousStructurePoints + structurePointsRestorePerRepairStage;
            newStructurePoints = Math.Min(structurePointsMax, newStructurePoints);
            publicState.StructurePointsCurrent = newStructurePoints;

            Logger.Important(
                string.Format("Repaired a vehicle: {0}: {1:F0} -> {2:F0}/{3:F0} structure points",
                              vehicle,
                              previousStructurePoints,
                              newStructurePoints,
                              structurePointsMax),
                character);

            // notify other players in scope
            var soundPosition = vehicle.Position;
            using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(vehicle, tempPlayers);
            tempPlayers.Remove(character);

            this.CallClient(tempPlayers.AsList(),
                            _ => _.ServerRemote_OnVehicleRepairByOtherPlayer(soundPosition));
        }

        public void ServerRefreshEnergyMax(IDynamicWorldObject vehicle)
        {
            var privateState = GetPrivateState(vehicle);
            var fuelItemsContainer = privateState.FuelItemsContainer;

            var maxEnergy = 0u;
            foreach (var item in fuelItemsContainer.Items)
            {
                if (item.ProtoItem is IProtoItemWithDurability protoItemWithDurability)
                {
                    maxEnergy += protoItemWithDurability.DurabilityMax;
                }
            }

            privateState.CurrentEnergyMax = maxEnergy;
        }

        public void SharedApplyInput(
            IDynamicWorldObject vehicle,
            ICharacter character,
            PlayerCharacterPrivateState characterPrivateState,
            PlayerCharacterPublicState characterPublicState)
        {
            var characterIsOffline = !character.ServerIsOnline;
            // please note - input is a structure so actually we're implicitly copying it here
            var input = characterPrivateState.Input;
            // please note - applied input is a class and we're getting it by reference here
            var appliedInput = characterPublicState.AppliedInput;
            var hasRunningFlag = (input.MoveModes & CharacterMoveModes.ModifierRun) != 0;
            var isRunning = hasRunningFlag;

            var isDismountRequested = vehicle.GetPublicState<VehiclePublicState>().IsDismountRequested;
            double moveSpeed;

            if (characterIsOffline
                || (characterPrivateState.CurrentActionState?.IsBlocksMovement ?? false)
                || isDismountRequested)
            {
                // offline or current action blocks movement
                moveSpeed = 0;
                isRunning = false;
                input.MoveModes = CharacterMoveModes.None;
            }
            else
            {
                moveSpeed = this.StatMoveSpeed * GameplayConstants.VehicleMoveSpeedMultiplier;
                moveSpeed *= ProtoTile.SharedGetTileMoveSpeedMultiplier(vehicle.Tile);

                if (isRunning)
                {
                    var moveSpeedMultiplier = this.StatMoveSpeedRunMultiplier;
                    if (moveSpeedMultiplier > 1.0)
                    {
                        moveSpeed *= moveSpeedMultiplier;
                    }
                    else
                    {
                        isRunning = false;
                    }
                }

                moveSpeed *= this.SharedGetMoveSpeedMultiplier(vehicle, character);
            }

            if (!isRunning)
            {
                // cannot run - remove running flag
                input.MoveModes &= ~CharacterMoveModes.ModifierRun;
            }

            if (appliedInput.MoveModes == input.MoveModes
                && appliedInput.RotationAngleRad == input.RotationAngleRad
                && characterPublicState.AppliedInput.MoveSpeed == moveSpeed)
            {
                // input is not changed
                return;
            }

            // apply new input
            appliedInput.Set(input, moveSpeed);

            var moveModes = input.MoveModes;

            double directionX = 0, directionY = 0;
            if ((moveModes & CharacterMoveModes.Up) != 0)
            {
                directionY = 1;
            }

            if ((moveModes & CharacterMoveModes.Down) != 0)
            {
                directionY = -1;
            }

            if ((moveModes & CharacterMoveModes.Left) != 0)
            {
                directionX = -1;
            }

            if ((moveModes & CharacterMoveModes.Right) != 0)
            {
                directionX = 1;
            }

            if (directionX == 0
                && directionY == 0)
            {
                moveSpeed = 0;
            }

            Vector2D directionVector = (directionX, directionY);
            var moveAcceleration = directionVector.Normalized * this.PhysicsBodyAccelerationCoef * moveSpeed;
            var friction = this.PhysicsBodyFriction;

            if (IsServer)
            {
                Server.World.SetDynamicObjectPhysicsMovement(vehicle,
                                                             moveAcceleration,
                                                             targetVelocity: moveSpeed);
                vehicle.PhysicsBody.Friction = friction;
            }
            else // if client
            {
                if (ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled)
                {
                    Client.World.SetDynamicObjectPhysicsMovement(vehicle,
                                                                 moveAcceleration,
                                                                 targetVelocity: moveSpeed);
                    vehicle.PhysicsBody.Friction = friction;
                }
            }
        }

        public bool SharedCanEditOwners(IWorldObject worldObject, ICharacter byOwner)
        {
            return this.HasOwnersList;
        }

        public override bool SharedCanInteract(ICharacter character, IDynamicWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (IsClient)
            {
                // client cannot perform owner's check so it will wait for the server's response
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character)
                || WorldObjectOwnersSystem.SharedIsOwner(character, worldObject))
            {
                return true;
            }

            if (writeToLog)
            {
                WorldObjectOwnersSystem.ServerNotifyNotOwner(character,
                                                             worldObject,
                                                             isFactionAccess: !string.IsNullOrEmpty(
                                                                                  GetPublicState(worldObject).ClanTag));
            }

            return false;
        }

        public virtual IItemsContainer SharedGetHotbarItemsContainer(IDynamicWorldObject vehicle)
        {
            return null;
        }

        public virtual double SharedGetMoveSpeedMultiplier(IDynamicWorldObject vehicle, ICharacter characterPilot)
        {
            return 1.0;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0, this.VehicleWorldHeight / 2.2);
        }

        public void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out IProtoCharacterSkeleton protoSkeleton,
            out double scale)
        {
            scale = 1;
            this.SharedGetSkeletonProto(gameObject, out var result, ref scale);
            protoSkeleton = result;
        }

        public override bool SharedIsInsideCharacterInteractionArea(
            ICharacter character,
            IDynamicWorldObject worldObject,
            bool writeToLog,
            CollisionGroup requiredCollisionGroup = null)
        {
            if (!base.SharedIsInsideCharacterInteractionArea(character,
                                                             worldObject,
                                                             writeToLog,
                                                             requiredCollisionGroup))
            {
                return false;
            }

            // distance check
            var isTooFar = worldObject.Position.DistanceSquaredTo(character.Position)
                           > this.MaxDistanceToInteract * this.MaxDistanceToInteract;
            if (!isTooFar)
            {
                return true;
            }

            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - too far",
                    character);

                if (IsClient)
                {
                    ClientOnCannotInteract(worldObject, CoreStrings.Notification_TooFar, isOutOfRange: true);
                }
            }

            return false;
        }

        public bool SharedIsTechUnlocked(ICharacter character, bool allowIfAdmin = true)
        {
            if (this.listedInTechNodes is null
                || this.listedInTechNodes.Count == 0)
            {
                return this.IsAutoUnlocked;
            }

            if (allowIfAdmin
                && CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            var techs = character.SharedGetTechnologies();
            foreach (var node in this.listedInTechNodes)
            {
                if (techs.SharedIsNodeUnlocked(node))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (PveSystem.SharedIsPve(false)
                && GetPublicState((IDynamicWorldObject)targetObject).PilotCharacter is null
                && (weaponCache.Character is null
                    || (weaponCache.Character.ProtoGameObject is IProtoCharacterMob protoCharacterMob
                        && !protoCharacterMob.IsBoss)))
            {
                // in PvE only a boss could damage a vehicle without a pilot
                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            if (weaponCache.ProtoWeapon is ItemNoWeapon)
            {
                // cannot hurt a vehicle with fists
                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       damagePostMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        public VehicleCanBuildCheckResult SharedPlayerCanBuild(ICharacter character)
        {
            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            var protoWorldObject = currentInteractionObject?.ProtoWorldObject;
            if (!(protoWorldObject is IProtoVehicleAssemblyBay
                      protoVehicleAssemblyBay))
            {
                return VehicleCanBuildCheckResult.NotInteractingWithVehicleBay;
            }

            if (IsServer
                && LandClaimSystem.SharedIsUnderRaidBlock(character, (IStaticWorldObject)currentInteractionObject))
            {
                return VehicleCanBuildCheckResult.BaseUnderRaidblock;
            }

            if (!this.SharedIsTechUnlocked(character))
            {
                return VehicleCanBuildCheckResult.TechIsNotResearched;
            }

            if (!this.SharedPlayerHasRequiredItemsToBuild(character))
            {
                return VehicleCanBuildCheckResult.RequiredItemsMissing;
            }

            if (IsServer
                && !this.ServerBaseHasEnoughPowerToBuild((IStaticWorldObject)currentInteractionObject)
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return VehicleCanBuildCheckResult.NotEnoughPower;
            }

            if (protoVehicleAssemblyBay.SharedIsBaySpaceBlocked(
                (IStaticWorldObject)currentInteractionObject))
            {
                return VehicleCanBuildCheckResult.NeedsFreeSpace;
            }

            return VehicleCanBuildCheckResult.Success;
        }

        public VehicleCanRepairCheckResult SharedPlayerCanRepairInVehicleAssemblyBay(ICharacter character)
        {
            var vehicle = InteractionCheckerSystem.SharedGetCurrentInteraction(character)
                              as IDynamicWorldObject;
            if (vehicle?.ProtoWorldObject != this)
            {
                return VehicleCanRepairCheckResult.NotInteractingWithVehicle;
            }

            this.VerifyGameObject(vehicle);

            var publicState = GetPublicState(vehicle);
            if (publicState.StructurePointsCurrent >= this.SharedGetStructurePointsMax(vehicle))
            {
                return VehicleCanRepairCheckResult.RepairIsNotRequired;
            }

            var vehicleAssemblyBay = VehicleSystem.SharedFindVehicleAssemblyBayNearby(character);
            if (vehicleAssemblyBay is null)
            {
                return VehicleCanRepairCheckResult.VehicleIsNotInsideVehicleAssemblyBay;
            }

            if (!this.SharedPlayerHasRequiredItemsToRepair(character))
            {
                return VehicleCanRepairCheckResult.RequiredItemsMissing;
            }

            if (IsServer
                && !this.ServerBaseHasEnoughPowerToRepair(vehicleAssemblyBay)
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return VehicleCanRepairCheckResult.NotEnoughPower;
            }

            return VehicleCanRepairCheckResult.Success;
        }

        public bool SharedPlayerHasRequiredItemsToBuild(ICharacter character, bool allowIfAdmin = true)
        {
            return InputItemsHelper.SharedPlayerHasRequiredItems(character, this.BuildRequiredItems, allowIfAdmin);
        }

        public bool SharedPlayerHasRequiredItemsToRepair(ICharacter character, bool allowIfAdmin = true)
        {
            return InputItemsHelper.SharedPlayerHasRequiredItems(character,
                                                                 this.RepairStageRequiredItems,
                                                                 allowIfAdmin);
        }

        protected static void ServerTryClaimGroundContainerWithDroppedGoods(
            IItemsContainer objectGroundContainer,
            ICharacter pilotCharacter,
            TVehiclePrivateState privateState)
        {
            if (objectGroundContainer is null)
            {
                return;
            }

            // apply PvE object claim to prevent other players from picking up the cargo
            if (pilotCharacter is not null)
            {
                WorldObjectClaimSystem.ServerTryClaim(objectGroundContainer.OwnerAsStaticObject,
                                                      pilotCharacter,
                                                      WorldObjectClaimDuration.DroppedGoods);
                return;
            }

            var ownerName = privateState.Owners.FirstOrDefault();
            if (ownerName is null)
            {
                return;
            }

            var ownerCharacter = Server.Characters.GetPlayerCharacter(ownerName);
            if (ownerCharacter is null)
            {
                return;
            }

            WorldObjectClaimSystem.ServerTryClaim(objectGroundContainer.OwnerAsStaticObject,
                                                  ownerCharacter,
                                                  WorldObjectClaimDuration.DroppedGoods);
        }

        protected virtual ITextureResource ClientCreateIcon()
        {
            return new TextureResource("Vehicles/" + this.ShortId);
        }

        protected virtual BaseClientComponentLightSource ClientCreateLightSource(
            IDynamicWorldObject vehicle)
        {
            return ClientLighting.CreateLightSourceSpot(
                vehicle.ClientSceneObject,
                this.VehicleLightConfig);
        }

        protected virtual void ClientDeinitializeVehicle(IDynamicWorldObject gameObject)
        {
        }

        protected sealed override void ClientInitialize(ClientInitializeData data)
        {
            // preload all the explosion spritesheets
            foreach (var textureAtlasResource in this.DestroyedExplosionPreset.SpriteAtlasResources)
            {
                Client.Rendering.PreloadTextureAsync(textureAtlasResource);
            }

            var vehicle = data.GameObject;
            var publicState = data.PublicState;
            var clientState = data.ClientState;

            var pilotCharacter = publicState.PilotCharacter;
            if (pilotCharacter is not null
                && pilotCharacter.IsInitialized)
            {
                // re-initialize pilot character physics
                pilotCharacter.ProtoWorldObject.SharedCreatePhysics(pilotCharacter);
            }

            this.ClientSetupRendering(data);
            this.ClientInitializeVehicle(data);

            var componentArmorBar = vehicle.ClientSceneObject.AddComponent<ComponentVehicleArmorBarManager>();
            componentArmorBar.IsDisplayedOnlyOnMouseOver = publicState.PilotCharacter is null;
            componentArmorBar.IsArmorBarDisplayedWhenPiloted = this.IsArmorBarDisplayedWhenPiloted;
            componentArmorBar.Setup(vehicle);

            if (pilotCharacter is not null
                && pilotCharacter.IsInitialized
                && pilotCharacter.IsCurrentClientCharacter
                && vehicle.ClientHasPrivateState)
            {
                this.ClientSetupCurrentPlayerUI(vehicle);
            }

            //// subscribe on pilot change
            //// (it works for other players only
            //// as for current player it will immediately call
            //// initialize method due to entering/leaving the private scope of the vehicle)
            publicState.ClientSubscribe(
                _ => _.PilotCharacter,
                _ =>
                {
                    // force re-initialize the vehicle
                    vehicle.ClientInitialize();
                },
                clientState);
        }

        protected virtual void ClientInitializeVehicle(ClientInitializeData data)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual void ClientSetupCurrentPlayerUI(IDynamicWorldObject vehicle)
        {
        }

        protected virtual void ClientSetupRendering(ClientInitializeData data)
        {
            var vehicle = data.GameObject;
            var publicState = data.PublicState;
            var clientState = data.ClientState;

            clientState.SkeletonRenderer?.Destroy();
            clientState.SkeletonRenderer = null;

            clientState.RendererShadow?.Destroy();
            clientState.RendererShadow = null;

            if (publicState.PilotCharacter is not null)
            {
                // pilot character will use the vehicle's skeleton
                return;
            }

            this.SharedGetSkeletonProto(vehicle,
                                        out var protoSkeleton,
                                        out var scaleMultiplier);

            if (protoSkeleton is null)
            {
                return;
            }

            // create skeleton renderer
            var skeletonRenderer = Api.Client.Rendering.CreateSkeletonRenderer(
                vehicle,
                skeletonResources: new[]
                {
                    protoSkeleton.SkeletonResourceFront,
                    protoSkeleton.SkeletonResourceBack
                },
                defaultSkeleton: protoSkeleton.SkeletonResourceFront,
                defaultLoopedAnimationName: "Idle",
                positionOffset: (0, 0),
                worldScale: scaleMultiplier
                            * ((ProtoCharacterSkeleton)protoSkeleton).WorldScale,
                speedMultiplier: protoSkeleton.SpeedMultiplier);

            protoSkeleton.OnSkeletonCreated(skeletonRenderer);

            // create shadow renderer
            clientState.RendererShadow = ((ProtoCharacterSkeleton)protoSkeleton)
                .ClientCreateShadowRenderer(vehicle,
                                            scaleMultiplier);

            clientState.SkeletonRenderer = skeletonRenderer;

            this.ClientSetupSkeleton(vehicle,
                                     protoSkeleton,
                                     skeletonRenderer,
                                     skeletonComponents: new List<IClientComponent>());
        }

        protected virtual void ClientTryDestroyCurrentPlayerUI(IDynamicWorldObject gameObject)
        {
            var clientState = GetClientState(gameObject);
            clientState.UIElementsHolder?.Dispose();
            clientState.UIElementsHolder = null;
        }

        protected virtual void PrepareDismountPoints(List<Vector2D> dismountPoints)
        {
        }

        protected sealed override void PrepareProtoDynamicWorldObject()
        {
            base.PrepareProtoDynamicWorldObject();

            var requiredItemsBuild = new InputItems();
            var repairStageRequiredItems = new InputItems();
            this.PrepareProtoVehicle(requiredItemsBuild,
                                     repairStageRequiredItems,
                                     out var repairStagesCount);
            Api.Assert(repairStagesCount > 0, "The repair stages number should a positive number larger than zero");
            this.BuildRequiredItems = requiredItemsBuild.AsReadOnly();
            this.RepairStageRequiredItems = repairStageRequiredItems.AsReadOnly();
            this.RepairStagesCount = repairStagesCount;

            var dismountPoints = new List<Vector2D>();
            this.PrepareDismountPoints(dismountPoints);
            this.DismountPoints = dismountPoints;

            var lightConfig = new ItemLightConfig();
            this.PrepareProtoVehicleLightConfig(lightConfig);
            lightConfig.IsLightEnabled = lightConfig.Size.X > 0
                                         && lightConfig.Size.Y > 0;
            this.VehicleLightConfig = lightConfig;

            this.PrepareProtoVehicleDestroyedExplosionPreset(
                out var destroyedVehicleDamageRadius,
                out var destroyedExplosionPreset,
                out var destroyedVehicleDamageDescriptionCharacters);

            this.DestroyedExplosionPreset = destroyedExplosionPreset
                                            ?? throw new Exception("No explosion preset provided");

            this.DestroyedVehicleDamageRadius = destroyedVehicleDamageRadius;
            this.DestroyedVehicleDamageDescriptionCharacters = destroyedVehicleDamageDescriptionCharacters;
        }

        protected abstract void PrepareProtoVehicle(
            InputItems buildRequiredItems,
            InputItems repairStageRequiredItems,
            out int repairStagesCount);

        protected abstract void PrepareProtoVehicleDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters);

        protected virtual void PrepareProtoVehicleLightConfig(ItemLightConfig lightConfig)
        {
        }

        protected virtual void ServerExecuteVehicleExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DestroyedVehicleDamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: false,
                isDamageThroughObstacles: false,
                callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance);

            double CalcDamageCoefByDistance(double distance)
            {
                var distanceThreshold = 0.5;
                if (distance <= distanceThreshold)
                {
                    return 1;
                }

                distance -= distanceThreshold;
                distance = Math.Max(0, distance);

                var maxDistance = this.DestroyedVehicleDamageRadius;
                maxDistance -= distanceThreshold;
                maxDistance = Math.Max(0, maxDistance);

                return 1 - Math.Min(distance / maxDistance, 1);
            }
        }

        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var pilotCharacter = data.PublicState.PilotCharacter;
            pilotCharacter?.ProtoWorldObject.SharedCreatePhysics(pilotCharacter);

            this.ServerSetUpdateRate(data.GameObject, isRare: pilotCharacter is null);
            this.ServerInitializeVehicle(data);
        }

        protected virtual void ServerInitializeVehicle(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;

            WorldObjectOwnersSystem.ServerInitialize(worldObject);

            // setup cargo items container
            var cargoItemsContainer = privateState.CargoItemsContainer;
            if (cargoItemsContainer is not null)
            {
                // container already created - update it
                Server.Items.SetContainerType(cargoItemsContainer, this.CargoItemsContainerType);
                Server.Items.SetSlotsCount(cargoItemsContainer, slotsCount: this.CargoItemsSlotsCount);
            }
            else
            {
                cargoItemsContainer = Server.Items.CreateContainer(
                    owner: worldObject,
                    itemsContainerType: this.CargoItemsContainerType,
                    slotsCount: this.CargoItemsSlotsCount);

                privateState.CargoItemsContainer = cargoItemsContainer;
            }

            // setup fuel items container
            var fuelItemsContainer = privateState.FuelItemsContainer;
            if (fuelItemsContainer is not null)
            {
                // container already created - update it
                Server.Items.SetContainerType(fuelItemsContainer, this.FuelItemsContainerType);
                Server.Items.SetSlotsCount(fuelItemsContainer, slotsCount: this.FuelItemsSlotsCount);
            }
            else
            {
                fuelItemsContainer = Server.Items.CreateContainer(
                    owner: worldObject,
                    itemsContainerType: this.FuelItemsContainerType,
                    slotsCount: this.FuelItemsSlotsCount);

                privateState.FuelItemsContainer = fuelItemsContainer;
            }
        }

        protected override void ServerOnDynamicObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            var vehicle = (IDynamicWorldObject)targetObject;

            // dismount the pilot before the explosion (so it will receive the explosion's damage)
            try
            {
                var pilot = GetPublicState(vehicle).PilotCharacter;
                if (pilot is not null)
                {
                    VehicleSystem.ServerCharacterExitCurrentVehicle(pilot, force: true);
                }
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }

            // explode
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetCharactersInRadius(vehicle.TilePosition,
                                               scopedBy,
                                               radius: this.DestroyedExplosionNetworkRadius,
                                               onlyPlayers: true);

            this.CallClient(scopedBy.AsList(),
                            _ => _.ClientRemote_VehicleExploded(vehicle.Position));

            SharedExplosionHelper.ServerExplode(
                character:
                null, // yes, no damaging character here otherwise it will not receive the damage if staying close
                protoExplosive: null,
                protoWeapon: null,
                explosionPreset: this.DestroyedExplosionPreset,
                epicenterPosition: vehicle.Position,
                damageDescriptionCharacters: this.DestroyedVehicleDamageDescriptionCharacters,
                physicsSpace: targetObject.PhysicsBody.PhysicsSpace,
                executeExplosionCallback: this.ServerExecuteVehicleExplosion);

            // destroy the vehicle completely after short explosion delay
            ServerTimersSystem.AddAction(
                this.DestroyedExplosionPreset.ServerDamageApplyDelay,
                () => base.ServerOnDynamicObjectZeroStructurePoints(weaponCache,
                                                                    byCharacter,
                                                                    targetObject));
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var privateState = data.PrivateState;
            var publicState = data.PublicState;
            var pilotCharacter = publicState.PilotCharacter;
            if (pilotCharacter is null)
            {
                // lights should be off if no pilot inside
                publicState.IsLightsEnabled = false;

                // remember how long the vehicle is standing without the pilot
                privateState.ServerTimeSinceLastUse += data.DeltaTime;

                if (!data.GameObject.ServerIsRareUpdateRate
                    && (privateState.IsInGarage
                        || !InteractionCheckerSystem.SharedHasAnyInteraction(data.GameObject)))
                {
                    // has no pilot, not in garage and no interactions
                    this.ServerSetUpdateRate(data.GameObject, isRare: true);
                }
            }
            else
            {
                privateState.ServerTimeSinceLastUse = 0;

                if (pilotCharacter.ServerIsOnline)
                {
                    privateState.ServerTimeSincePilotOffline = 0;
                }
                else
                {
                    privateState.ServerTimeSincePilotOffline += data.DeltaTime;

                    if (privateState.ServerTimeSincePilotOffline >= ThresholdDurationRemoveOfflinePlayerFromVehicle)
                    {
                        // offline player is removed from the vehicle as it was too long in offline
                        // (required to ensure players will not use all energy in vehicle due to idle consumption)
                        VehicleSystem.ServerCharacterExitCurrentVehicle(pilotCharacter, force: true);
                    }
                }
            }

            this.ServerUpdateVehicle(data);
        }

        protected virtual void ServerUpdateVehicle(ServerUpdateData data)
        {
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            if (IsServer
                && data.PrivateState.IsInGarage)
            {
                return;
            }

            var physicsBody = data.PhysicsBody;
            physicsBody.IsNotPushable = true;

            var scale = 1.0;
            this.SharedGetSkeletonProto(data.GameObject, out var skeleton, ref scale);
            if (skeleton is null)
            {
                return;
            }

            skeleton.CreatePhysics(physicsBody);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (scale != 1.0)
            {
                // apply non-default scale
                physicsBody.ApplyShapesScale(scale);
            }
        }

        protected abstract void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale);

        private void ClientRemote_OnVehicleBuiltByOtherPlayer(Vector2D position)
        {
            Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleBuilt, position);
        }

        private void ClientRemote_VehicleExploded(Vector2D position)
        {
            Logger.Important(this + " exploded at " + position);
            SharedExplosionHelper.ClientExplode(position,
                                                this.DestroyedExplosionPreset,
                                                this.DestroyedExplosionVolume);
        }

        private void ClientShowNotificationCannotBuild(VehicleCanBuildCheckResult checkResult)
        {
            string message;
            switch (checkResult)
            {
                case VehicleCanBuildCheckResult.Success:
                    return;

                case VehicleCanBuildCheckResult.NotEnoughPower:
                    message = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                    break;

                case VehicleCanBuildCheckResult.BaseUnderRaidblock:
                    LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(
                        ClientCurrentCharacterHelper.Character);
                    return;

                default:
                    message = checkResult.GetDescription();
                    break;
            }

            NotificationSystem.ClientShowNotification(
                Notification_CannotBuildVehicleTitle,
                message,
                color: NotificationColor.Bad,
                this.Icon);
        }

        private void ClientShowNotificationCannotRepair(VehicleCanRepairCheckResult checkResult)
        {
            string message;
            switch (checkResult)
            {
                case VehicleCanRepairCheckResult.Success:
                    return;

                case VehicleCanRepairCheckResult.NotEnoughPower:
                    message = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                    break;

                /*case VehicleCanRepairCheckResult.BaseUnderRaidblock:
                    LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(
                        ClientCurrentCharacterHelper.Character);
                    return;*/

                default:
                    message = checkResult.GetDescription();
                    break;
            }

            NotificationSystem.ClientShowNotification(
                Notification_CannotRepairVehicleTitle,
                message,
                color: checkResult == VehicleCanRepairCheckResult.RepairIsNotRequired
                           ? NotificationColor.Neutral
                           : NotificationColor.Bad,
                this.Icon);
        }

        private bool ServerBaseHasEnoughPowerToBuild(IStaticWorldObject vehicleAssemblyBay)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicleAssemblyBay.Bounds);
            return PowerGridSystem.ServerBaseHasCharge(areasGroup, this.BuildRequiredElectricityAmount);
        }

        private bool ServerBaseHasEnoughPowerToRepair(IStaticWorldObject vehicleAssemblyBay)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicleAssemblyBay.Bounds);
            return PowerGridSystem.ServerBaseHasCharge(areasGroup, this.RepairRequiredElectricityAmount);
        }

        private VehicleCanBuildCheckResult ServerRemote_BuildVehicle()
        {
            var character = ServerRemoteContext.Character;
            var checkResult = this.SharedPlayerCanBuild(character);
            if (checkResult != VehicleCanBuildCheckResult.Success)
            {
                Logger.Warning("Cannot build a vehicle: " + checkResult, character);
                return checkResult;
            }

            var vehicleAssemblyBay =
                (IStaticWorldObject)InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                Api.Logger.Important(character
                                     + " is in creative mode - no items and electricity deduction on construction");
            }
            else
            {
                InputItemsHelper.ServerDestroyItems(character, this.BuildRequiredItems);
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicleAssemblyBay.Bounds);
                PowerGridSystem.ServerDeductBaseCharge(areasGroup, this.BuildRequiredElectricityAmount);
            }

            var vehicle = Server.World.CreateDynamicWorldObject(
                this,
                position: vehicleAssemblyBay.TilePosition.ToVector2D()
                          + ((IProtoVehicleAssemblyBay)vehicleAssemblyBay.ProtoWorldObject)
                          .PlatformCenterWorldOffset);

            this.ServerOnBuilt(vehicle, character);

            Logger.Important("Built a vehicle: " + vehicle, character);

            // notify other players in scope
            var soundPosition = vehicleAssemblyBay.TilePosition.ToVector2D()
                                + ((IProtoVehicleAssemblyBay)vehicleAssemblyBay.ProtoGameObject)
                                .PlatformCenterWorldOffset;
            using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(vehicle, tempPlayers);
            tempPlayers.Remove(character);

            this.CallClient(tempPlayers.AsList(),
                            _ => _.ClientRemote_OnVehicleBuiltByOtherPlayer(soundPosition));

            return VehicleCanBuildCheckResult.Success;
        }

        private void ServerRemote_OnVehicleRepairByOtherPlayer(Vector2D position)
        {
            Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleRepair, position);
        }

        private VehicleCanRepairCheckResult ServerRemote_RepairVehicle()
        {
            var character = ServerRemoteContext.Character;
            var vehicle = InteractionCheckerSystem.SharedGetCurrentInteraction(character) as IDynamicWorldObject;
            this.VerifyGameObject(vehicle);

            var checkResult = this.SharedPlayerCanRepairInVehicleAssemblyBay(character);
            if (checkResult != VehicleCanRepairCheckResult.Success)
            {
                Logger.Warning("Cannot repair a vehicle: " + checkResult, character);
                return checkResult;
            }

            var vehicleAssemblyBay = VehicleSystem.SharedFindVehicleAssemblyBayNearby(character);
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                Api.Logger.Important(character + " is in creative mode - no items deduction on repair");
            }
            else
            {
                InputItemsHelper.ServerDestroyItems(character, this.RepairStageRequiredItems);
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicleAssemblyBay.Bounds);
                PowerGridSystem.ServerDeductBaseCharge(areasGroup, this.RepairRequiredElectricityAmount);
            }

            this.ServerOnRepair(vehicle, character);
            return VehicleCanRepairCheckResult.Success;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.5)]
        private void ServerRemote_SetCurrentVehicleLightsMode(bool setIsActive)
        {
            if (!this.HasVehicleLights)
            {
                return;
            }

            var character = ServerRemoteContext.Character;
            var vehicle = character.SharedGetCurrentVehicle();
            if (vehicle is null)
            {
                Logger.Warning($"Player is not piloting any vehicle now: {character}");
                return;
            }

            this.VerifyGameObject(vehicle);

            var publicState = GetPublicState(vehicle);
            if (publicState.PilotCharacter != character)
            {
                Logger.Warning(
                    $"Only pilot can toggle the vehicles light: {vehicle} player is not the pilot: {character}");
                return;
            }

            if (publicState.IsLightsEnabled == setIsActive)
            {
                Logger.Warning($"The same lights mode is already set: isActive={setIsActive} for {vehicle}", character);
                return;
            }

            publicState.IsLightsEnabled = setIsActive;
            Logger.Info($"Player switched vehicle light mode: {vehicle}, isActive={setIsActive}", character);
        }
    }

    public abstract class ProtoVehicle
        : ProtoVehicle
            <VehiclePrivateState,
                VehiclePublicState,
                VehicleClientState>
    {
    }
}