namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public partial class PlayerCharacter
        : ProtoCharacter
            <PlayerCharacterPrivateState,
                PlayerCharacterPublicState,
                PlayerCharacterClientState>
    {
        private const double InteractionRadius = 1.5;

        private static readonly Lazy<SkeletonHumanFemale> SkeletonHumanFemale
            = new Lazy<SkeletonHumanFemale>(GetProtoEntity<SkeletonHumanFemale>);

        private static readonly Lazy<SkeletonHumanMale> SkeletonHumanMale
            = new Lazy<SkeletonHumanMale>(GetProtoEntity<SkeletonHumanMale>);

        public override float CharacterWorldHeight => 1.5f;

        public override string Name => "Player character";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SoftTissues;

        public virtual double StatDefaultFoodMax => 100;

        public override double StatDefaultHealthMax => 100;

        public override double StatDefaultStaminaMax => 100;

        public virtual double StatDefaultWaterMax => 100;

        public override double StatHealthRegenerationPerSecond
            => 2.0 / 60.0; // 2 health points per minute (1 every 30 seconds)

        public override double StatMoveSpeed => 1.5;

        public override double StatMoveSpeedRunMultiplier => 1.25;

        // 5 stamina per second (with total 100 stamina and without skills this gives 20 seconds sprint, 40 with athletics skill, even more with survival skill)
        public virtual double StatRunningStaminaConsumptionUsePerSecond => 5.0;

        public override double StatStaminaRegenerationPerSecond => 5;

        /// <summary>
        /// This is a special client method which is invoked for current player character only
        /// to add a very faint light around it in case the character doesn't have any active light item
        /// (otherwise nights will be too dark).
        /// </summary>
        public static BaseClientComponentLightSource ClientCreateDefaultLightSource(ICharacter character)
        {
            return ClientLighting.CreateLightSourceSpot(
                Client.Scene.GetSceneObject(character),
                color: Colors.White.WithAlpha(0x28),
                size: 10,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (0, 0.5));
        }

        public override void ClientDeinitialize(ICharacter character)
        {
            if (character.IsCurrentClientCharacter)
            {
                // reset game
                BootstrapperClientGame.Init(null);
                WindowRespawn.EnsureClosed();
            }
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

            ClientCurrentCharacterLagPredictionManager.UpdatePosition(forceReset, currentCharacter);
        }

        public override void SharedRefreshFinalCacheIfNecessary(ICharacter character)
        {
            this.ServerRebuildFinalCacheIfNeeded(GetPrivateState(character),
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

            var publicState = data.SyncPublicState;
            var clientState = data.ClientState;
            ClientCharacterEquipmentHelper.ClientRefreshEquipment(character, clientState, publicState);

            if (character.IsCurrentClientCharacter)
            {
                this.ClientInitializeCurrentCharacter(data);
            }
            else
            {
                this.ClientInitializeOtherCharacter(data);
            }

            // subscribe on head style change
            publicState.ClientSubscribe(
                _ => _.FaceStyle,
                _ => ResetRendering(resetSkeleton: true),
                clientState);

            // subscribe on gender change
            publicState.ClientSubscribe(
                _ => _.IsMale,
                _ => ResetRendering(),
                clientState);

            // subscribe on player character public action change
            publicState.ClientSubscribe(
                _ => _.CurrentPublicActionState,
                _ => this.ClientRefreshCurrentPublicActionState(character),
                clientState);

            this.ClientRefreshCurrentPublicActionState(character);

            publicState.ClientSubscribe(
                _ => _.SelectedHotbarItem,
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
                                         .PlaySound(ItemSound.Deselect, limitOnePerFrame: false);

                currentItemIdleSoundEmitter.Stop();
                var currentItem = publicState.SelectedHotbarItem;
                var itemSoundPreset = currentItem?
                                      .ProtoItem
                                      .SharedGetItemSoundPreset();
                previousSelectedProtoItem = currentItem?.ProtoItem;

                if (itemSoundPreset == null)
                {
                    return;
                }

                itemSoundPreset.PlaySound(ItemSound.Select, limitOnePerFrame: false);
                var idleSoundResource = itemSoundPreset.GetSound(ItemSound.Idle);
                if (idleSoundResource != null)
                {
                    currentItemIdleSoundEmitter.SoundResource = idleSoundResource;
                    currentItemIdleSoundEmitter.Delay = 0.1;
                    currentItemIdleSoundEmitter.Play();
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
            var publicState = data.SyncPublicState;
            ClientCharacterEquipmentHelper.ClientRefreshEquipment(character, clientState, publicState);

            if (!character.IsCurrentClientCharacter)
            {
                base.ClientUpdate(data);
                return;
            }

            // next code is only for the current client character
            var privateState = data.SyncPrivateState;
            if (!publicState.IsDead)
            {
                SharedRefreshSelectedHotbarItem(character, privateState);
            }

            this.ServerRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);

            ClientCharacterAnimationHelper.ClientUpdateAnimation(
                character,
                clientState,
                publicState);

            if (publicState.IsDead)
            {
                // dead - stops processing character
                WindowRespawn.EnsureOpened();
                return;
            }

            // character is alive - can process its actions
            WindowRespawn.EnsureClosed();
            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(character, privateState.WeaponState, data.DeltaTime);
            // update current action state (if any)
            privateState.CurrentActionState?.SharedUpdate(data.DeltaTime);
            // consumes/restores stamina
            CharacterStaminaSystem.SharedUpdate(character, publicState, privateState, (float)data.DeltaTime);
        }

        protected sealed override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.FoodMax,  this.StatDefaultFoodMax);
            effects.AddValue(this, StatName.WaterMax, this.StatDefaultWaterMax);

            effects.AddValue(this,
                             StatName.RunningStaminaConsumptionPerSecond,
                             this.StatRunningStaminaConsumptionUsePerSecond);

            effects.AddValue(this,
                             StatName.LearningPointsRetainedAfterDeath,
                             SkillLearning.LearningPointsRetainedAfterDeathBaseValue);
        }

        protected override void PrepareProtoCharacter()
        {
            base.PrepareProtoCharacter();
            CraftingMechanics.ServerNonManufacturingRecipeCrafted +=
                craftingQueueItem =>
                {
                    if (!(craftingQueueItem.GameObject is ICharacter character)
                        || character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                    {
                        return;
                    }

                    var exp = SkillCrafting.ExperiencePerItemCrafted;
                    // we're adding extra experience matching the crafted recipe original duration
                    exp += SkillCrafting.ExperiencePerItemCraftedRecipeDuration
                           * craftingQueueItem.Recipe.OriginalDuration;
                    character.ServerAddSkillExperience<SkillCrafting>(exp);
                };
        }

        protected override void ServerInitializeCharacter(ServerInitializeData data)
        {
            base.ServerInitializeCharacter(data);

            var character = data.GameObject;
            var privateState = data.PrivateState;

            // re-select hotbar slot
            SharedSelectHotbarSlotId(character, privateState.SelectedHotbarSlotId);
        }

        protected override void ServerInitializeCharacterFirstTime(ServerInitializeData data)
        {
            base.ServerInitializeCharacterFirstTime(data);

            var character = data.GameObject;
            var publicState = data.PublicState;

            if (!Api.IsEditor)
            {
                NewbieProtectionSystem.ServerRegisterNewbie(character);
            }

            publicState.IsMale = true;
            publicState.FaceStyle = SharedCharacterFaceStylesProvider
                                    .GetForGender(publicState.IsMale)
                                    .GenerateRandomFace();

            ServerPlayerSpawnManager.ServerAddTorchItemIfNoItems(character);

            if (!Api.IsEditor)
            {
                return;
            }

            // the game is run as Editor
            // auto pwn in editor mode
            GetProtoEntity<ConsoleAdminPwn>().Execute(player: character);
            // add all the skills
            GetProtoEntity<ConsoleSkillsSetAll>().Execute(player: character);
            // add all the technologies
            GetProtoEntity<ConsoleTechAddAll>().Execute(player: character);
            // add all the quests (and complete them)
            GetProtoEntity<ConsoleQuestCompleteAll>().Execute(player: character);
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
        }

        protected void ServerRebuildFinalCacheIfNeeded(
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
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            publicState.IsOnline = character.IsOnline;

            // update selected hotbar item
            SharedRefreshSelectedHotbarItem(character, privateState);

            if (publicState.IsDead)
            {
                // dead - stops processing character
                Server.Characters.SetMoveSpeed(character, 0);
                Server.Characters.SetVelocity(character, Vector2D.Zero);
                return;
            }

            // character is alive
            this.ServerRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);

            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(character, privateState.WeaponState, data.DeltaTime);
            // update current action state (if any)
            privateState.CurrentActionState?.SharedUpdate(data.DeltaTime);
            // update crafting queue
            CraftingMechanics.ServerUpdate(privateState.CraftingQueue, data.DeltaTime);
            // consumes/restores stamina
            CharacterStaminaSystem.SharedUpdate(character, publicState, privateState, (float)data.DeltaTime);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            if (GetPublicState(data.GameObject).IsDead)
            {
                // do not create any physics for dead character
                return;
            }

            base.SharedCreatePhysics(data);

            // add interaction area
            var characterInteractionRadius = InteractionRadius;
            if (IsServer)
            {
                // allow a little bigger interaction area on server
                // (because the client position could differ from server position with some threshold)
                characterInteractionRadius += 2
                                              * ClientCurrentCharacterLagPredictionManager
                                                  .MaxPredictionErrorDistanceToInterpolateWhenStaying;

                // increase the interaction radius a bit to counter
                // the networked positions quantization due to coordinate packing
                // (in tile position is encoded as 0-255 (byte.MaxValue) for each axis)
                characterInteractionRadius += 1 / 255.0;
            }

            data.PhysicsBody.AddShapeCircle(
                center: (0, 0.65),
                radius: characterInteractionRadius,
                group: CollisionGroups.CharacterInteractionArea);
        }

        protected override void SharedGetSkeletonProto(
            [CanBeNull] ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            if (character == null)
            {
                protoSkeleton = SkeletonHumanMale.Value;
                return;
            }

            var isMale = GetPublicState(character).IsMale;
            protoSkeleton = isMale
                                ? (SkeletonHuman)SkeletonHumanMale.Value
                                : (SkeletonHuman)SkeletonHumanFemale.Value;
        }

        private void ClientInitializeCurrentCharacter(ClientInitializeData data)
        {
            var character = data.GameObject;
            var privateState = data.SyncPrivateState;
            var publicState = data.SyncPublicState;

            this.ServerRebuildFinalCacheIfNeeded(privateState, publicState);

            var sceneObject = Client.Scene.GetSceneObject(character);
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
            SharedSelectHotbarSlotId(character, privateState.SelectedHotbarSlotId);

            WindowRespawn.EnsureClosed();
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        private void ClientInitializeOtherCharacter(ClientInitializeData data)
        {
            IComponentAttachedControl nicknameDisplay = null;

            var publicState = data.SyncPublicState;
            publicState.ClientSubscribe(
                _ => _.IsOnline,
                _ => Refresh(),
                data.ClientState);

            publicState.ClientSubscribe(
                _ => _.IsDead,
                _ => Refresh(),
                data.ClientState);

            Refresh();

            void Refresh()
            {
                nicknameDisplay?.Destroy();
                nicknameDisplay = null;
                if (publicState.IsDead)
                {
                    return;
                }

                // setup nickname display
                var character = data.GameObject;
                var nicknameDisplayControl = new NicknameDisplay();

                nicknameDisplayControl.Setup(character, publicState.IsOnline);
                nicknameDisplay = Client.UI.AttachControl(
                    character,
                    nicknameDisplayControl,
                    positionOffset: (0, this.CharacterWorldHeight + 0.05),
                    isFocusable: false);
            }
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