namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Actions;
    using AtomicTorch.CBND.CoreMod.ClientComponents.AmbientSound;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStamina;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public partial class PlayerCharacter
        : ProtoCharacter
            <PlayerCharacterPrivateState,
                PlayerCharacterPublicState,
                PlayerCharacterClientState>
    {
        /// <summary>
        /// Uncomment to let CryoFall Editor launch with the character customization menu.
        /// </summary>
        public const bool IsEditorModeAutoSelectingAppearance = true;

        private const double InteractionRadius = 1.5;

        public static readonly Lazy<PlayerCharacter> LazyInstance
            = new(Api.GetProtoEntity<PlayerCharacter>);

        private static readonly Lazy<SkeletonHumanFemale> SkeletonHumanFemale
            = new(GetProtoEntity<SkeletonHumanFemale>);

        private static readonly Lazy<SkeletonHumanMale> SkeletonHumanMale
            = new(GetProtoEntity<SkeletonHumanMale>);

        public static PlayerCharacter Instance => LazyInstance.Value;

        public override float CharacterWorldHeight => 1.5f;

        // This is the center of the character's melee hitbox.
        // It's necessary to ensure the equal distance in every way the character could shot and damage another player.
        public override float CharacterWorldWeaponOffsetMelee { get; }
            = (float)(ProtoCharacterSkeletonHuman.MeleeHitboxOffset
                      + ProtoCharacterSkeletonHuman.MeleeHitboxHeight / 2);

        // This is the center of the character's ranged hitbox.
        // It's necessary to ensure the equal distance in every way the character could shot and damage another player.
        public override float CharacterWorldWeaponOffsetRanged { get; }
            = (float)(ProtoCharacterSkeletonHuman.RangedHitboxOffset
                      + ProtoCharacterSkeletonHuman.RangedHitboxHeight / 2);

        public override string Name => "Player character";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public override double PhysicsBodyAccelerationCoef => 10;

        public override double PhysicsBodyFriction => 30;

        public virtual double StatDefaultFoodMax => 100;

        public override double StatDefaultHealthMax => 100;

        public virtual double StatDefaultStaminaMax => 100;

        public virtual double StatDefaultWaterMax => 100;

        public override double StatHealthRegenerationPerSecond
            => 2.0 / 60.0; // 2 health points per minute (1 every 30 seconds)

        public override double StatMoveSpeed => 2.25;

        public override double StatMoveSpeedRunMultiplier => 1.25;

        // 5 stamina per second (with total 100 stamina and without skills this gives 20 seconds sprint, 40 with athletics skill, even more with survival skill)
        public virtual double StatRunningStaminaConsumptionUsePerSecond => 5.0;

        public virtual double StatStaminaRegenerationPerSecond => 5;

        /// <summary>
        /// This is a special client method which is invoked for current player character only
        /// to add a very faint light around it in case the character doesn't have any active light item
        /// (otherwise nights will be too dark).
        /// </summary>
        public static BaseClientComponentLightSource ClientCreateDefaultLightSource(ICharacter character)
        {
            return ClientLighting.CreateLightSourceSpot(
                character.ClientSceneObject,
                color: Colors.White.WithAlpha(0x28),
                size: 10,
                // the light is very faint so we're using a smaller logical size
                logicalSize: 6,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (0, 0.5));
        }

        public override void ClientDeinitialize(ICharacter character)
        {
            if (character.IsCurrentClientCharacter)
            {
                // reset game
                BootstrapperClientGame.Init(null);
                MenuRespawn.EnsureClosed();
                MenuCharacterCreation.Reset();
            }

            var clientState = GetClientState(character);
            clientState.CurrentPublicActionState?.InvokeClientDeinitialize();
            clientState.CurrentPublicActionState = null;
        }

        public override void ClientOnServerPhysicsUpdate(
            IWorldObject worldObject,
            Vector2D serverPosition,
            bool forceReset)
        {
            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            if (!ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled
                || worldObject != currentCharacter)
            {
                base.ClientOnServerPhysicsUpdate(worldObject, serverPosition, forceReset);
                return;
            }

            // current character - use prediction system
            var publicState = GetPublicState(currentCharacter);
            if (publicState.IsDead)
            {
                // don't use prediction system - player is dead
                // don't change the position (don't move the corpse!)
                //base.ClientOnServerPhysicsUpdate(worldObject, serverPosition, forceReset);
                return;
            }

            var currentVehicle = publicState.CurrentVehicle;
            if (currentVehicle is null
                || !currentVehicle.IsInitialized)
            {
                ClientCurrentCharacterLagPredictionManager.UpdatePosition(forceReset, currentCharacter);
            }
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (!base.SharedOnDamage(weaponCache,
                                     targetObject,
                                     damagePreMultiplier,
                                     damagePostMultiplier,
                                     out obstacleBlockDamageCoef,
                                     out damageApplied))
            {
                return false;
            }

            if (IsServer)
            {
                var character = (ICharacter)targetObject;
                var vehicle = GetPublicState(character).CurrentVehicle;
                if (vehicle is not null)
                {
                    var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                    protoVehicle.ServerOnPilotDamage(weaponCache,
                                                     vehicle,
                                                     character,
                                                     damageApplied);
                }
            }

            return true;
        }

        public override void SharedRefreshFinalCacheIfNecessary(ICharacter character)
        {
            this.SharedRebuildFinalCacheIfNeeded(GetPrivateState(character),
                                                 GetPublicState(character));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var character = data.GameObject;
            IProtoItem previousSelectedProtoItem = null;
            var currentItemIdleSoundEmitter = Client.Audio.CreateSoundEmitter(character,
                                                                              SoundResource.NoSound,
                                                                              isLooped: true,
                                                                              volume: 0.5f);

            var publicState = data.PublicState;
            var clientState = data.ClientState;
            ClientCharacterEquipmentHelper.ClientRefreshEquipment(character, clientState, publicState);

            if (character.IsCurrentClientCharacter)
            {
                this.ClientInitializeCurrentCharacter(data);
                this.ClientInitializeOtherCharacter(data);
            }
            else
            {
                this.ClientInitializeOtherCharacter(data);
            }

            // subscribe on head style change
            publicState.ClientSubscribe(
                _ => _.FaceStyle,
                _ => ResetRendering(resetSkeleton: false),
                clientState);

            // subscribe on gender change
            publicState.ClientSubscribe(
                _ => _.IsMale,
                _ => ResetRendering(resetSkeleton: true),
                clientState);

            // subscribe on head equipment visibility change
            publicState.ClientSubscribe(
                _ => _.IsHeadEquipmentHiddenForSelfAndPartyMembers,
                _ => ResetRendering(resetSkeleton: false),
                clientState);

            // subscribe on vehicle change
            publicState.ClientSubscribe(
                _ => _.CurrentVehicle,
                _ =>
                {
                    // reset input history for lag prediction
                    CurrentCharacterInputHistory.Instance.Clear();
                    // re-create physics
                    this.SharedCreatePhysics(character);
                    // re-select current item
                    SharedForceRefreshCurrentItem(ClientCurrentCharacterHelper.Character);
                    // reset rendering
                    ResetRendering(resetSkeleton: false);
                },
                clientState);

            // subscribe on player character public action change
            publicState.ClientSubscribe(
                _ => _.CurrentPublicActionState,
                _ => this.ClientRefreshCurrentPublicActionState(character),
                clientState);

            this.ClientRefreshCurrentPublicActionState(character);

            publicState.ClientSubscribe(
                _ => _.SelectedItem,
                _ =>
                {
                    // selected different item
                    RefreshCurrentSelectedItem();
                },
                clientState);

            publicState.ClientSubscribe(
                _ => _.IsDead,
                _ =>
                {
                    // re-create physics on death state change
                    this.SharedCreatePhysics(character);
                    ResetRendering();

                    if (character.IsCurrentClientCharacter
                        && publicState.IsDead)
                    {
                        Menu.CloseAll();
                    }
                },
                clientState);

            RefreshCurrentSelectedItem();

            void RefreshCurrentSelectedItem()
            {
                previousSelectedProtoItem?.SharedGetItemSoundPreset()
                                         .PlaySound(ItemSound.Deselect, character);

                currentItemIdleSoundEmitter.Stop();
                var currentItem = publicState.SelectedItem;
                var itemSoundPreset = currentItem?
                                      .ProtoItem
                                      .SharedGetItemSoundPreset();
                previousSelectedProtoItem = currentItem?.ProtoItem;

                if (itemSoundPreset is not null)
                {
                    itemSoundPreset.PlaySound(ItemSound.Select, character);

                    var idleSoundResource = itemSoundPreset.GetSound(ItemSound.Idle);
                    if (idleSoundResource is not null)
                    {
                        currentItemIdleSoundEmitter.SoundResource = idleSoundResource;
                        currentItemIdleSoundEmitter.Delay = 0.1;
                        currentItemIdleSoundEmitter.Play();
                    }
                }
            }

            void ResetRendering(bool resetSkeleton = false)
            {
                // reset hash to force updating equipment/skeleton in the next frame
                clientState.LastEquipmentContainerHash = null;
                if (resetSkeleton)
                {
                    clientState.CurrentProtoSkeleton = null;
                }
            }
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            var character = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;

            if (!character.IsCurrentClientCharacter)
            {
                ClientCharacterEquipmentHelper.ClientRefreshEquipment(character, clientState, publicState);
                base.ClientUpdate(data);
                return;
            }

            // next code is only for the current client character
            var privateState = data.PrivateState;
            if (!publicState.IsDead)
            {
                SharedRefreshSelectedHotbarItem(character, privateState);
            }

            ClientCharacterEquipmentHelper.ClientRefreshEquipment(character, clientState, publicState);

            this.SharedRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);

            ClientCharacterAnimationHelper.ClientUpdateAnimation(
                character,
                clientState,
                publicState);

            if (!privateState.IsAppearanceSelected)
            {
                MenuCharacterCreation.Open();
                return;
            }

            if (publicState.IsDead)
            {
                // dead - stops processing character
                MenuRespawn.EnsureOpened();
                return;
            }

            // character is alive - can process its actions
            MenuRespawn.EnsureClosed();
            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(character, privateState.WeaponState, data.DeltaTime);
            // update current action state (if any)
            privateState.CurrentActionState?.SharedUpdate(data.DeltaTime);
            // consumes/restores stamina
            CharacterStaminaSystem.SharedUpdate(character, publicState, privateState, data.DeltaTime);
        }

        protected sealed override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.FoodMax, this.StatDefaultFoodMax)
                   .AddValue(this, StatName.WaterMax,                     this.StatDefaultWaterMax)
                   .AddValue(this, StatName.StaminaMax,                   this.StatDefaultStaminaMax)
                   .AddValue(this, StatName.StaminaRegenerationPerSecond, this.StatStaminaRegenerationPerSecond);

            effects.AddValue(this,
                             StatName.RunningStaminaConsumptionPerSecond,
                             this.StatRunningStaminaConsumptionUsePerSecond);

            effects.AddValue(this,
                             StatName.CraftingQueueMaxSlotsCount,
                             SkillCrafting.BaseCraftingSlotsCount);

            effects.AddPercent(this,
                               StatName.TinkerTableBonus,
                               SkillMaintenance.BaseTinkerTableBonus);

            effects.AddPercent(this,
                               StatName.FishingSuccess,
                               SkillFishing.BaseFishCatchChancePercents);

            if (LandClaimSystemConstants.SharedLandClaimsNumberLimitIncrease > 0)
            {
                effects.AddValue(this,
                                 StatName.LandClaimsMaxNumber,
                                 LandClaimSystemConstants.SharedLandClaimsNumberLimitIncrease);
            }
        }

        protected override void ServerInitializeCharacter(ServerInitializeData data)
        {
            base.ServerInitializeCharacter(data);

            var character = data.GameObject;
            var privateState = data.PrivateState;

            // re-select hotbar slot
            SharedSelectHotbarSlotId(character, privateState.SelectedHotbarSlotId, isByPlayer: false);

            character.ProtoGameObject.ServerSetUpdateRate(character, isRare: !character.ServerIsOnline);
        }

        protected override void ServerInitializeCharacterFirstTime(ServerInitializeData data)
        {
            base.ServerInitializeCharacterFirstTime(data);

            var character = data.GameObject;
            var publicState = data.PublicState;

            if (!Api.IsEditor)
            {
                NewbieProtectionSystem.ServerRegisterNewbie(character);

                publicState.IsMale = 1 == RandomHelper.Next(0, maxValueExclusive: 2); // male/female ratio: 50/50
                publicState.FaceStyle = SharedCharacterFaceStylesProvider
                                        .GetForGender(publicState.IsMale)
                                        .GenerateRandomFace();
            }
            else // if Editor
            {
                publicState.IsMale = true;
                publicState.FaceStyle = SharedCharacterFaceStylesProvider
                                        .GetForGender(publicState.IsMale)
                                        .GetDefaultFaceInEditor();
                data.PrivateState.IsAppearanceSelected = IsEditorModeAutoSelectingAppearance;
            }

            ServerPlayerSpawnManager.ServerAddTorchItemIfNoItems(character);

            if (!Api.IsEditor)
            {
                return;
            }

            // the game is run as Editor
            // auto pwn in editor mode
            ConsoleCommandsSystem.SharedGetCommand<ConsoleAdminPwn>().Execute(player: character);
            // add all the skills
            ConsoleCommandsSystem.SharedGetCommand<ConsoleSkillsSetAll>().Execute(player: character);
            // add all the technologies
            ConsoleCommandsSystem.SharedGetCommand<ConsoleTechAddAll>().Execute(player: character);

            this.SharedRebuildFinalCacheIfNeeded(data.PrivateState, publicState);

            // add all the quests (and complete them)
            ConsoleCommandsSystem.SharedGetCommand<ConsoleQuestCompleteAll>().Execute(player: character);
        }

        protected override void ServerPrepareCharacter(ServerInitializeData data)
        {
            base.ServerPrepareCharacter(data);

            var character = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            privateState.ServerLastAckClientInputId = 0;

            privateState.ServerInitState(character);
            publicState.ServerInitState(character);

            privateState.Technologies.ServerInit();
            privateState.Quests.ServerInit();
            privateState.Achievements.ServerInit();
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            publicState.IsOnline = character.ServerIsOnline;

            // update selected hotbar item
            SharedRefreshSelectedHotbarItem(character, privateState);

            if (publicState.IsDead)
            {
                VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);

                // dead - stops processing character
                var world = Server.World;
                world.SetDynamicObjectMoveSpeed(character, 0);
                world.SetDynamicObjectPhysicsMovement(character,
                                                      accelerationVector: Vector2D.Zero,
                                                      targetVelocity: 0);
                character.PhysicsBody.Friction = 100000;
                world.StopPhysicsBody(character.PhysicsBody);
                return;
            }

            // character is alive
            this.SharedRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);

            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(character, privateState.WeaponState, data.DeltaTime);
            // update current action state (if any)
            privateState.CurrentActionState?.SharedUpdate(data.DeltaTime);
            // update crafting queue
            CraftingMechanics.ServerUpdate(privateState.CraftingQueue, data.DeltaTime);
            // consumes/restores stamina
            CharacterStaminaSystem.SharedUpdate(character, publicState, privateState, data.DeltaTime);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var publicState = GetPublicState(data.GameObject);
            if (publicState.IsDead)
            {
                // do not create any physics for dead character
                return;
            }

            var physicsBody = data.PhysicsBody;
            var currentVehicle = publicState.CurrentVehicle;
            var protoVehicle = currentVehicle?.ProtoGameObject as IProtoVehicle;
            if (protoVehicle is not null)
            {
                protoVehicle.SharedGetSkeletonProto(currentVehicle, out var skeleton, out _);
                if (skeleton is not null)
                {
                    // do not create any physics for a character in vehicle (vehicle has its own physics)
                    physicsBody.AttachedToPhysicsBody = currentVehicle.PhysicsBody;
                    // but ensure the physics body is enabled even if it doesn't have physics shapes)
                    physicsBody.IsEnabled = true;
                    return;
                }
            }

            base.SharedCreatePhysics(data);

            // add interaction area
            var characterInteractionRadius = InteractionRadius;
            if (IsServer)
            {
                // allow a little bigger interaction area on server
                // (because the client position could differ from the server position with some threshold)
                characterInteractionRadius += 2.2
                                              * ClientCurrentCharacterLagPredictionManager
                                                  .MaxPredictionErrorDistanceToInterpolateWhenStaying;

                // increase the interaction radius a bit to counter
                // the networked position quantization due to coordinate packing
                // (in-tile position is encoded as 0-255 (byte.MaxValue) integer value for each axis)
                characterInteractionRadius += 1 / 255.0;
            }

            if (protoVehicle is null)
            {
                physicsBody.AddShapeCircle(
                    center: (0, 0.65),
                    radius: characterInteractionRadius,
                    group: CollisionGroups.CharacterInteractionArea);
                physicsBody.UseInstantVelocityDirectionChange = true;
            }
            else // has a vehicle
            {
                physicsBody.RemoveShapesOfGroup(CollisionGroups.Default);
                if (!ReferenceEquals(CollisionGroups.Default,
                                     CollisionGroups.CharacterOrVehicle))
                {
                    physicsBody.RemoveShapesOfGroup(CollisionGroups.CharacterOrVehicle);
                }

                physicsBody.AttachedToPhysicsBody = currentVehicle.PhysicsBody;
            }
        }

        protected override void SharedGetSkeletonProto(
            [CanBeNull] ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            if (character is null)
            {
                protoSkeleton = SkeletonHumanMale.Value;
                return;
            }

            var publicState = GetPublicState(character);
            var isMale = publicState.IsMale;
            protoSkeleton = isMale
                                ? (ProtoCharacterSkeletonHuman)SkeletonHumanMale.Value
                                : (ProtoCharacterSkeletonHuman)SkeletonHumanFemale.Value;
        }

        protected void SharedRebuildFinalCacheIfNeeded(
            PlayerCharacterPrivateState privateState,
            PlayerCharacterPublicState publicState)
        {
            if (!privateState.FinalStatsCache.IsDirty
                && publicState.ContainerEquipment.StateHash == privateState.ContainerEquipmentLastStateHash)
            {
                return;
            }

            // rebuild stats cache
            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(
                this.ProtoCharacterDefaultEffects,
                publicState,
                privateState);

            if (IsServer)
            {
                CraftingMechanics.ServerRecalculateTimeToFinish(privateState.CraftingQueue);
            }
        }

        private void ClientInitializeCurrentCharacter(ClientInitializeData data)
        {
            var character = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            this.SharedRebuildFinalCacheIfNeeded(privateState, publicState);

            var sceneObject = character.ClientSceneObject;
            sceneObject
                .AddComponent<ComponentPlayerInputUpdater>()
                .Setup(character);

            sceneObject
                .AddComponent<ComponentAmbientSoundManager>();

            BootstrapperClientGame.Init(character);

            // add current public action progress display/watcher
            sceneObject
                .AddComponent<ClientComponentActionWithProgressWatcher>()
                .Setup(privateState);

            // add skills watcher
            sceneObject
                .AddComponent<ClientComponentSkillsWatcher>()
                .Setup(privateState);

            // add technologies watcher
            sceneObject
                .AddComponent<ClientComponentTechnologiesWatcher>()
                .Setup(privateState);

            CurrentCharacterInputHistory.Instance.Clear();

            this.CallServer(_ => _.ServerRemote_ResetLastInputAck());

            // re-select hotbar slot
            SharedSelectHotbarSlotId(character, privateState.SelectedHotbarSlotId, isByPlayer: false);

            MenuRespawn.EnsureClosed();
        }

        private void ClientInitializeOtherCharacter(ClientInitializeData data)
        {
            // nothing here now but could be useful for modders
        }

        private void ClientRefreshCurrentPublicActionState(ICharacter character)
        {
            var actionState = GetPublicState(character).CurrentPublicActionState;
            var clientState = GetClientState(character);
            if (clientState.CurrentPublicActionState == actionState)
            {
                // action state is not changed (should never happen as we're listening to the change event)
                return;
            }

            clientState.CurrentPublicActionState?.InvokeClientOnCompleted();
            clientState.CurrentPublicActionState = actionState;
            clientState.CurrentPublicActionState?.InvokeClientOnStart(character);
        }
    }
}