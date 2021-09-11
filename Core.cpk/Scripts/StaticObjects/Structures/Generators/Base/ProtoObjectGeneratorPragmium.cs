namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectGeneratorPragmium
        : ProtoObjectGenerator
          <ObjectGeneratorPragmiumPrivateState,
              ObjectGeneratorPragmiumPublicState,
              StaticObjectClientState>,
          IProtoObjectPsiSourceCustom
    {
        public const string ErrorCannotStartReactor_Description_Format
            = "Place at least one {0} to start the reactor.";

        public const string ErrorCannotStartReactor_Title = "Cannot start reactor";

        /// <summary>
        /// To ensure a good granularity of the decay (durability is an integer value),
        /// the items decay is applied rarely.
        /// </summary>
        private const int ItemDecayIntervalDuration = 10;

        public IReadOnlyList<ProtoItemWithCount> BuildAdditionalReactorRequiredItems { get; private set; }

        public override bool IsGeneratorAlwaysOn => true;

        public override bool IsRelocatable => false;

        public abstract byte ItemSlotsCountPerReactor { get; }

        /// <summary>
        /// This and other two Psi-related parameters are not used directly so they're zero here
        /// (see the override method).
        /// </summary>
        public double PsiIntensity => 0;

        public double PsiRadiusMax => 0;

        public double PsiRadiusMin => 0;

        public abstract byte ReactorsCountInitial { get; }

        public abstract byte ReactorsCountMax { get; }

        public override double ServerUpdateIntervalSeconds => 0.5;

        public abstract double ShutdownDuration { get; }

        public abstract double StartupDuration { get; }

        protected abstract double PsiEmissionDistanceMultiplier { get; }

        protected abstract double PsiEmissionDistanceOffset { get; }

        public static bool SharedIsReactorActiveForItemsContainer(
            IStaticWorldObject worldObjectReactor,
            IItemsContainer itemsContainer)
        {
            foreach (var reactorPrivateState in GetPrivateState(worldObjectReactor).ReactorStates)
            {
                if (reactorPrivateState.ItemsContainer == itemsContainer)
                {
                    return reactorPrivateState.IsEnabled
                           || reactorPrivateState.ActivationProgressPercents > 0;
                }
            }

            // reactor doesn't exist
            return false;
        }

        public void ClientBuildReactor(IStaticWorldObject worldObjectGenerator, byte reactorIndex)
        {
            if (!InputItemsHelper.SharedPlayerHasRequiredItems(ClientCurrentCharacterHelper.Character,
                                                               this.BuildAdditionalReactorRequiredItems,
                                                               noCheckInCreativeMode: true))
            {
                NotificationSystem.ClientShowNotification(
                    ObjectTinkerTable.ErrorMessage_ComponentItemsRequried,
                    color: NotificationColor.Bad,
                    icon: this.Icon);
                return;
            }

            ProtoEntityRemoteExtensions.CallServer(this,
                                                   _ => _.ServerRemote_BuildReactor(
                                                       worldObjectGenerator,
                                                       reactorIndex));
        }

        public void ClientSetReactorMode(
            IStaticWorldObject worldObjectGenerator,
            byte reactorIndex,
            bool isEnabled)
        {
            var reactorPrivateState = SharedGetReactorPrivateState(worldObjectGenerator, reactorIndex);
            if (reactorPrivateState.IsEnabled == isEnabled)
            {
                // no change necessary
                return;
            }

            if (isEnabled
                && !SharedCanActivateReactor(worldObjectGenerator, reactorPrivateState, logErrors: true))
            {
                return;
            }

            ProtoEntityRemoteExtensions.CallServer(this,
                                                   _ => _.ServerRemote_SetReactorMode(
                                                       worldObjectGenerator,
                                                       reactorIndex,
                                                       isEnabled));
        }

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            var publicState = GetPublicState(worldObject);
            if (publicState.StructurePointsCurrent <= 0)
            {
                // already awaiting destruction
                return;
            }

            base.ServerApplyDecay(worldObject, deltaTime);

            if (publicState.StructurePointsCurrent > 0)
            {
                return;
            }

            // destroyed by decay
            ServerResetReactorContents(worldObject);
        }

        public double ServerCalculatePsiIntensity(IWorldObject worldObject, ICharacter character)
        {
            var maxPsiIntensityToCharacter = 0.0;
            var emissionDistanceOffset = this.PsiEmissionDistanceOffset;
            var emissionDistanceMultiplier = this.PsiEmissionDistanceMultiplier;

            var reactorPrivateStates = GetPrivateState((IStaticWorldObject)worldObject).ReactorStates;
            for (var reactorIndex = 0; reactorIndex < reactorPrivateStates.Length; reactorIndex++)
            {
                var reactorPrivateState = reactorPrivateStates[reactorIndex];
                if (reactorPrivateState is null)
                {
                    continue;
                }

                if (reactorPrivateState.ActivationProgressPercents <= 0)
                {
                    continue;
                }

                var reactorEmission = emissionDistanceMultiplier
                                      * this.SharedGetPsiEmissionLevelCurrent(reactorPrivateState);
                if (reactorEmission <= 0)
                {
                    continue;
                }

                var reactorWorldPosition = worldObject.TilePosition.ToVector2D()
                                           + this.SharedGetReactorWorldPositionOffset(reactorIndex);

                var distance = character.Position.DistanceTo(reactorWorldPosition);

                var maxDistance = emissionDistanceOffset + reactorEmission;
                var minDistance = emissionDistanceOffset;
                var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
                if (distanceCoef >= 1)
                {
                    // too far
                    continue;
                }

                var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);
                intensity = Math.Pow(intensity, 0.1);

                if (intensity > maxPsiIntensityToCharacter)
                {
                    maxPsiIntensityToCharacter = intensity;
                }

                //Logger.Dev($"Calculated Psi intensity to reactor #{reactorIndex}: {intensity:F3}; distance: {distance:F3}");
            }

            return maxPsiIntensityToCharacter;
        }

        public bool ServerIsPsiSourceActive(IWorldObject worldObject)
        {
            this.SharedGetElectricityProduction((IStaticWorldObject)worldObject,
                                                out var currentProduction,
                                                out _);
            return currentProduction > 0;
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            foreach (var reactorState in GetPrivateState(gameObject).ReactorStates)
            {
                if (reactorState?.ItemsContainer is null)
                {
                    continue;
                }

                ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                    gameObject.OccupiedTile,
                    reactorState.ItemsContainer);
            }
        }

        public override void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction)
        {
            maxProduction = 0;
            currentProduction = 0;
            foreach (var reactorPrivateState in GetPrivateState(worldObject).ReactorStates)
            {
                if (reactorPrivateState is null)
                {
                    continue;
                }

                var outputValue = reactorPrivateState.Stats.OutputValue;
                maxProduction += outputValue;

                var activationProgress = reactorPrivateState.ActivationProgressPercents / 100.0;
                currentProduction += outputValue * activationProgress;
            }
        }

        public virtual double SharedGetPsiEmissionLevelCurrent(
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState)
        {
            if (reactorPrivateState is null)
            {
                return 0;
            }

            var emissionLevel = reactorPrivateState.Stats.PsiEmissionLevel;
            if (emissionLevel <= 0)
            {
                return 0;
            }

            return emissionLevel
                   * Math.Min(100, 2 * reactorPrivateState.ActivationProgressPercents)
                   / 100.0;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            // reinitialize structure when a reactor is added
            publicState.ClientSubscribe(
                _ => _.ReactorStates,
                _ =>
                {
                    this.SoundPresetObject.PlaySound(ObjectSound.Place);
                    worldObject.ClientInitialize();
                },
                data.ClientState);

            for (var index = 0; index < publicState.ReactorStates.Length; index++)
            {
                var reactorState = publicState.ReactorStates[index];
                if (reactorState is null)
                {
                    continue;
                }

                var reactorSpriteRenderer = Client.Rendering.CreateSpriteRenderer(worldObject);
                this.ClientSetupReactorSpriteRenderer(worldObject, reactorSpriteRenderer, reactorState, index);
            }
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            WindowGeneratorPragmium window = null;
            window = WindowGeneratorPragmium.Open(
                new ViewModelWindowGeneratorPragmium(
                    data.GameObject,
                    data.PrivateState,
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    () => window.CloseWindow()));
            return window;
        }

        protected abstract void ClientSetupReactorSpriteRenderer(
            IStaticWorldObject worldObject,
            IComponentSpriteRenderer reactorSpriteRenderer,
            ObjectGeneratorPragmiumReactorPublicState reactorState,
            int index);

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            var buildAdditionalReactor = new InputItems();
            this.PrepareConstructionConfigGenerator(tileRequirements,
                                                    build,
                                                    repair,
                                                    buildAdditionalReactor,
                                                    out category);
            this.BuildAdditionalReactorRequiredItems = buildAdditionalReactor.AsReadOnly();
        }

        protected abstract void PrepareConstructionConfigGenerator(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            InputItems inputItems,
            out ProtoStructureCategory category);

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObjectGenerator = data.GameObject;
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            var reactorPrivateStates = privateState.ReactorStates;
            if (reactorPrivateStates?.Length != this.ReactorsCountMax)
            {
                privateState.ReactorStates = null;
                publicState.ReactorStates = null;
                reactorPrivateStates = new ObjectGeneratorPragmiumReactorPrivateState[this.ReactorsCountMax];
            }

            var reactorPublicStates = publicState.ReactorStates
                                      ?? new ObjectGeneratorPragmiumReactorPublicState[this.ReactorsCountMax];

            for (var index = 0; index < reactorPrivateStates.Length; index++)
            {
                var reactorPrivateState = reactorPrivateStates[index];
                if (reactorPrivateState is null
                    && index < this.ReactorsCountInitial)
                {
                    reactorPrivateState = new ObjectGeneratorPragmiumReactorPrivateState();
                    reactorPrivateStates[index] = reactorPrivateState;
                }

                if (reactorPrivateState is not null)
                {
                    this.ServerSetupReactorPrivateState(worldObjectGenerator, reactorPrivateState);
                    reactorPublicStates[index] = new ObjectGeneratorPragmiumReactorPublicState();
                }
            }

            // force refresh over the network and properly binding the state owner object
            privateState.ReactorStates = reactorPrivateStates.ToArray();
            publicState.ReactorStates = reactorPublicStates.ToArray();
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            if (byCharacter is not null
                && byCharacter.SharedGetPlayerSelectedHotbarItemProto() is not ProtoItemToolCrowbar)
            {
                // somebody destroyed the nuclear reactor (it was not disassembled)
                // reset the contents on the reactor containers and spawn an explosion object
                ServerResetReactorContents((IStaticWorldObject)targetObject);
            }

            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);
        }

        protected void ServerSetupReactorPrivateState(
            IStaticWorldObject worldObjectGenerator,
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState)
        {
            var itemsContainer = reactorPrivateState.ItemsContainer;
            var itemsSlotsCount = this.ItemSlotsCountPerReactor;
            if (itemsContainer is not null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerGeneratorPragmium>(
                owner: worldObjectGenerator,
                slotsCount: itemsSlotsCount);

            reactorPrivateState.ItemsContainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var privateReactorStates = data.PrivateState.ReactorStates;
            var publicReactorStates = data.PublicState.ReactorStates;

            for (byte reactorIndex = 0; reactorIndex < privateReactorStates.Length; reactorIndex++)
            {
                var reactorPrivateState = privateReactorStates[reactorIndex];
                if (reactorPrivateState is null)
                {
                    continue;
                }

                var itemsContainer = reactorPrivateState.ItemsContainer;
                if (reactorPrivateState.ServerItemsContainerLastStateHash
                    != itemsContainer.StateHash)
                {
                    var reactorPublicState = publicReactorStates[reactorIndex];
                    this.ServerRebuildReactorStats(reactorPrivateState, reactorPublicState);
                }

                ServerApplyItemsDecay(reactorPrivateState, data.DeltaTime);

                double deltaProgress;
                if (reactorPrivateState.IsEnabled)
                {
                    deltaProgress = data.DeltaTime / this.StartupDuration;
                }
                else
                {
                    deltaProgress = -data.DeltaTime / this.ShutdownDuration;
                }

                deltaProgress *= 100.0 / reactorPrivateState.Stats.StartupShutdownTimePercent;

                var previousActivationProgressPercents = reactorPrivateState.ActivationProgressPercents;
                var activationProgressPercents = previousActivationProgressPercents
                                                 + deltaProgress * 100;
                activationProgressPercents = MathHelper.Clamp(activationProgressPercents, 0, 100);

                reactorPrivateState.ActivationProgressPercents = activationProgressPercents;
                publicReactorStates[reactorIndex].ActivationProgressPercents
                    = activationProgressPercents is > 0 and <= 1
                          ? (byte)1 // at least 1% should be provided to start the active reactor animation immediately
                          : (byte)activationProgressPercents;

                if (!reactorPrivateState.IsEnabled
                    && previousActivationProgressPercents > 0
                    && activationProgressPercents == 0)
                {
                    ServerOnReactorShutdown(reactorPrivateState);
                }
            }
        }

        protected PragmiumReactorStatsData SharedGetReactorStats(
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState)
        {
            double outputValue = 0,
                   efficiencyPercents = 100,
                   fuelLifetimePercent = 100,
                   psiEmissionLevel = 0,
                   startupShutdownTimePercent = 100;

            foreach (var item in reactorPrivateState.ItemsContainer.Items)
            {
                switch (item.ProtoGameObject)
                {
                    case ItemReactorFuelRod protoRod:
                        psiEmissionLevel += protoRod.PsiEmissionLevel;
                        outputValue += protoRod.OutputElectricityPerSecond;
                        break;

                    case ProtoItemReactorModule protoModule:
                        efficiencyPercents += protoModule.EfficiencyModifierPercents;
                        fuelLifetimePercent += protoModule.FuelLifetimeModifierPercents;
                        psiEmissionLevel += protoModule.PsiEmissionModifierValue;
                        startupShutdownTimePercent += protoModule.StartupShutdownTimeModifierPercents;
                        break;
                }
            }

            var generatorRate = RateTimeDependentGeneratorsRate.SharedValue;

            // do not apply it here as the player should not see the change here! 
            //fuelLifetimePercent /= generatorRate; // apply generator rate (faster generation but faster fuel decay)
            fuelLifetimePercent = MathHelper.Clamp(fuelLifetimePercent, 1, ushort.MaxValue);

            efficiencyPercents = MathHelper.Clamp(efficiencyPercents, 0, ushort.MaxValue);

            // apply generator rate (faster generation and activation but faster fuel decay)
            startupShutdownTimePercent /= generatorRate;
            startupShutdownTimePercent = MathHelper.Clamp(startupShutdownTimePercent, 5, ushort.MaxValue);

            psiEmissionLevel = MathHelper.Clamp(psiEmissionLevel, 0, byte.MaxValue);

            outputValue *= efficiencyPercents / 100.0;
            outputValue *= generatorRate; // apply generator rate (faster generation but faster fuel decay)

            return new PragmiumReactorStatsData((ushort)fuelLifetimePercent,
                                                (ushort)efficiencyPercents,
                                                psiEmissionLevel,
                                                outputValue,
                                                (ushort)startupShutdownTimePercent);
        }

        protected abstract Vector2D SharedGetReactorWorldPositionOffset(int reactorIndex);

        private static void ServerApplyItemsDecay(
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState,
            double deltaTime)
        {
            var activationProgress = reactorPrivateState.ActivationProgressPercents / 100.0;
            if (activationProgress <= 0)
            {
                reactorPrivateState.ServerAccumulatedDecayDuration = 0;
                return;
            }

            using var tempItemsList = Api.Shared.WrapInTempList(reactorPrivateState.ItemsContainer.Items);
            if (tempItemsList.Count == 0)
            {
                return;
            }

            reactorPrivateState.ServerAccumulatedDecayDuration += deltaTime;
            if (reactorPrivateState.ServerAccumulatedDecayDuration < ItemDecayIntervalDuration)
            {
                return;
            }

            // let's apply accumulated decay
            reactorPrivateState.ServerAccumulatedDecayDuration -= ItemDecayIntervalDuration;
            var decayMultiplier = activationProgress * ItemDecayIntervalDuration;

            // apply generator rate (faster generation but faster fuel decay)
            decayMultiplier *= RateTimeDependentGeneratorsRate.SharedValue;

            var fuelDecayMultiplier = 100.0 / reactorPrivateState.Stats.FuelLifetimePercent;

            foreach (var item in tempItemsList.AsList())
            {
                var protoGameObject = item.ProtoGameObject;
                switch (protoGameObject)
                {
                    case ItemReactorFuelRod protoFuelRod:
                    {
                        var decayAmount = decayMultiplier
                                          * protoFuelRod.DurabilityMax
                                          / protoFuelRod.LifetimeDuration;

                        decayAmount *= fuelDecayMultiplier;

                        ItemDurabilitySystem.ServerModifyDurability(item, -(int)decayAmount);
                        break;
                    }

                    case ProtoItemReactorModule protoItemModule:
                    {
                        var decayAmount = decayMultiplier
                                          * protoItemModule.DurabilityMax
                                          / protoItemModule.LifetimeDuration;

                        // Don't allow for modules to decay completely.
                        // When the reactor completely shut downs, it will destroy all modules
                        // that have remaining durability below 1% (see ServerOnReactorShutdown).
                        var privateState = item.GetPrivateState<IItemWithDurabilityPrivateState>();
                        decayAmount = Math.Min(decayAmount, privateState.DurabilityCurrent - 1);

                        if (decayAmount > 0)
                        {
                            ItemDurabilitySystem.ServerModifyDurability(item, -(int)decayAmount);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// When the reactor completely shut downs, it will destroy all modules
        /// that have remaining durability below 1%.
        /// </summary>
        private static void ServerOnReactorShutdown(ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState)
        {
            foreach (var item in reactorPrivateState.ItemsContainer.Items.ToList())
            {
                if (item.ProtoGameObject is ProtoItemReactorModule
                    && ItemDurabilitySystem.SharedGetDurabilityPercent(item) < 1)
                {
                    ItemDurabilitySystem.ServerBreakItem(item);
                }
            }
        }

        private static void ServerResetReactorContents(IStaticWorldObject worldObject)
        {
            foreach (var reactorPrivateState in GetPrivateState(worldObject).ReactorStates)
            {
                if (reactorPrivateState?.ItemsContainer is null)
                {
                    continue;
                }

                Server.Items.DestroyContainer(reactorPrivateState.ItemsContainer);
                reactorPrivateState.ItemsContainer = null;
            }
        }

        private static bool SharedCanActivateReactor(
            IStaticWorldObject worldObjectGenerator,
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState,
            bool logErrors)
        {
            if (!reactorPrivateState.ItemsContainer.Items
                                    .Any(item => item.ProtoItem is ItemReactorFuelRod))
            {
                if (logErrors)
                {
                    Logger.Warning("Cannot activate reactor - no rods: " + worldObjectGenerator);

                    if (IsClient)
                    {
                        NotificationSystem.ClientShowNotification(
                            ErrorCannotStartReactor_Title,
                            message: string.Format(ErrorCannotStartReactor_Description_Format,
                                                   Api.GetProtoEntity<ItemReactorFuelRod>().Name),
                            NotificationColor.Bad,
                            GetProtoEntity<ItemReactorFuelRod>().Icon);
                    }
                }

                return false;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObjectGenerator);
            if (areasGroup is null)
            {
                // no power grid exists
                if (logErrors)
                {
                    Logger.Warning("Cannot activate reactor - no power grid exists: " + worldObjectGenerator);

                    if (IsClient)
                    {
                        NotificationSystem.ClientShowNotification(
                            ErrorCannotStartReactor_Title,
                            message: PowerGridSystem.SetPowerModeResult.NoPowerGridExist.GetDescription(),
                            NotificationColor.Bad,
                            worldObjectGenerator.ProtoStaticWorldObject.Icon);
                    }
                }

                return false;
            }

            return true;
        }

        private static ObjectGeneratorPragmiumReactorPrivateState SharedGetReactorPrivateState(
            IStaticWorldObject worldObjectGenerator,
            byte reactorIndex)
        {
            var privateState = GetPrivateState(worldObjectGenerator);
            var reactorPrivateStates = privateState.ReactorStates;
            var reactorPrivateState = reactorPrivateStates[reactorIndex];
            return reactorPrivateState
                   ?? throw new Exception($"The reactor is not built: #{reactorIndex} in {worldObjectGenerator}");
        }

        private void ServerRebuildReactorStats(
            ObjectGeneratorPragmiumReactorPrivateState reactorPrivateState,
            ObjectGeneratorPragmiumReactorPublicState reactorPublicState)
        {
            reactorPrivateState.Stats = this.SharedGetReactorStats(reactorPrivateState);
            reactorPrivateState.ServerItemsContainerLastStateHash = reactorPrivateState.ItemsContainer.StateHash;

            // verify that the reactor has at least a single fuel item, otherwise deactivate it
            var hasAnyFuel = false;

            foreach (var item in reactorPrivateState.ItemsContainer.Items)
            {
                if (item.ProtoItem is ItemReactorFuelRod)
                {
                    hasAnyFuel = true;
                    break;
                }
            }

            if (!hasAnyFuel
                && (reactorPrivateState.IsEnabled
                    || reactorPrivateState.ActivationProgressPercents > 0))
            {
                reactorPrivateState.IsEnabled = false;
                reactorPrivateState.ActivationProgressPercents = 0;
                reactorPublicState.ActivationProgressPercents = 0;
                //Logger.Info("Fuel depleted: " + reactorPrivateState.GameObject);
            }
        }

        [RemoteCallSettings(timeInterval: 1.0)]
        private void ServerRemote_BuildReactor(IStaticWorldObject worldObjectGenerator, byte reactorIndex)
        {
            this.VerifyGameObject(worldObjectGenerator);
            var character = ServerRemoteContext.Character;

            if (!this.SharedCanInteract(character, worldObjectGenerator, writeToLog: true))
            {
                return;
            }

            if (reactorIndex >= this.ReactorsCountMax)
            {
                throw new ArgumentOutOfRangeException(nameof(reactorIndex));
            }

            var privateState = GetPrivateState(worldObjectGenerator);
            var publicState = GetPublicState(worldObjectGenerator);

            var reactorPrivateStates = privateState.ReactorStates;
            var reactorPrivateState = reactorPrivateStates[reactorIndex];
            if (reactorPrivateState is not null)
            {
                throw new Exception($"The reactor is already built: #{reactorIndex} in {worldObjectGenerator}");
            }

            if (!InputItemsHelper.SharedPlayerHasRequiredItems(character,
                                                               this.BuildAdditionalReactorRequiredItems,
                                                               noCheckInCreativeMode: true))
            {
                throw new Exception($"Not enough items to build a reactor: #{reactorIndex} in {worldObjectGenerator}");
            }

            if (!CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                InputItemsHelper.ServerDestroyItems(character, this.BuildAdditionalReactorRequiredItems);
            }

            reactorPrivateState = new ObjectGeneratorPragmiumReactorPrivateState();
            reactorPrivateStates[reactorIndex] = reactorPrivateState;

            this.ServerSetupReactorPrivateState(worldObjectGenerator, reactorPrivateState);

            var reactorPublicStates = publicState.ReactorStates;
            reactorPublicStates[reactorIndex] = new ObjectGeneratorPragmiumReactorPublicState();

            // force refresh over the network and properly binding the state owner object
            privateState.ReactorStates = reactorPrivateStates.ToArray();
            publicState.ReactorStates = reactorPublicStates.ToArray();
        }

        [RemoteCallSettings(timeInterval: 0.333)]
        private void ServerRemote_SetReactorMode(
            IStaticWorldObject worldObjectGenerator,
            byte reactorIndex,
            bool isEnabled)
        {
            this.VerifyGameObject(worldObjectGenerator);
            var character = ServerRemoteContext.Character;

            if (!this.SharedCanInteract(character, worldObjectGenerator, writeToLog: true))
            {
                return;
            }

            if (reactorIndex >= this.ReactorsCountMax)
            {
                throw new ArgumentOutOfRangeException(nameof(reactorIndex));
            }

            var reactorPrivateState = SharedGetReactorPrivateState(worldObjectGenerator, reactorIndex);
            if (isEnabled
                && !SharedCanActivateReactor(worldObjectGenerator, reactorPrivateState, logErrors: true))
            {
                return;
            }

            reactorPrivateState.IsEnabled = isEnabled;
        }
    }
}