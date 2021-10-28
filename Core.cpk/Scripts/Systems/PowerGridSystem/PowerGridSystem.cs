namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class PowerGridSystem : ProtoSystem<PowerGridSystem>
    {
        public const double BaseCapacity = 10000.0;

        public const double PowerGridEfficiencyDropPercentPerExtraLandClaim = 2.0;

        /// <summary>
        /// The min bound for power grid efficiency is calculated for the base containing 3*3 land claims.
        /// (it's possible to build larger bases only on the local server)
        /// </summary>
        public const double PowerGridMinEfficiencyPercents = 100.0
                                                             - 8 * PowerGridEfficiencyDropPercentPerExtraLandClaim;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

        [RemoteEnum]
        public enum SetPowerModeResult : byte
        {
            Success = 0,

            [Description("Cannot interact—too far or building inaccessible.")]
            CannotInteractWithObject = 10,

            [Description("No power grid exists.")]
            NoPowerGridExist = 20,

            [Description("Not enough power in the power grid.")]
            NotEnoughPower = 30
        }

        public static void ClientInitializeConsumerOrProducer(IStaticWorldObject worldObject)
        {
            if ((worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer consumer
                 && consumer.ElectricityConsumptionPerSecondWhenActive > 0)
                || worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer
                {
                    IsGeneratorAlwaysOn: false
                })
            {
                ObjectPowerStateOverlay.CreateFor(worldObject);
            }
        }

        public static void ClientSetElectricityThresholds(
            IStaticWorldObject worldObject,
            ElectricityThresholdsPreset thresholds)
        {
            thresholds = thresholds.Normalize(
                isProducer: worldObject.ProtoGameObject is IProtoObjectElectricityProducer);

            if (thresholds.Equals(worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>()
                                             .ElectricityThresholds))
            {
                // no changes
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_SetElectricityThresholds(worldObject, thresholds));
        }

        public static async Task<bool> ClientSetPowerMode(bool isOn, IStaticWorldObject worldObject)
        {
            var result = await Instance.CallServer(_ => _.ServerRemote_SetPowerMode(worldObject, isOn));
            if (result == SetPowerModeResult.Success)
            {
                return true;
            }

            NotificationSystem.ClientShowNotification(result.GetDescription(),
                                                      color: NotificationColor.Bad,
                                                      icon: worldObject.ProtoStaticWorldObject.Icon);
            return false;
        }

        /// <summary>
        /// Injects power control UI into the window menu for electricity consumers and producers.
        /// </summary>
        public static void ClientTryInjectPowerSwitchUI(
            IStaticWorldObject worldObject,
            BaseUserControlWithWindow window)
        {
            if ((worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer consumer
                 && consumer.ElectricityConsumptionPerSecondWhenActive > 0)
                || (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer producer
                    && !producer.IsGeneratorAlwaysOn))
            {
                // proper consumer or producer object
            }
            else
            {
                return;
            }

            var powerSwitchControl = PowerSwitchControl.Create(worldObject);
            window.Window.AddExtensionControl(powerSwitchControl);
        }

        public static bool ServerBaseHasCharge(
            [CanBeNull] ILogicObject areasGroup,
            double amount)
        {
            if (areasGroup is null)
            {
                return false;
            }

            var grid = SharedGetPowerGrid(areasGroup);
            var gridPublicState = PowerGrid.GetPublicState(grid);
            return gridPublicState.ElectricityAmount >= amount;
        }

        public static void ServerDeductBaseCharge(
            ILogicObject areasGroup,
            double amountToDeduct)
        {
            if (areasGroup is null)
            {
                throw new ArgumentNullException(nameof(areasGroup), "Areas group is not found");
            }

            var grid = SharedGetPowerGrid(areasGroup);
            var gridPublicState = PowerGrid.GetPublicState(grid);
            var newAmount = gridPublicState.ElectricityAmount - amountToDeduct;
            if (newAmount < 0)
            {
                newAmount = 0;
            }

            gridPublicState.ElectricityAmount = newAmount;
            Logger.Info(
                $"Electricity deducted from power grid: {grid} - {amountToDeduct:F0}. Total amount now: {newAmount:F0}");
        }

        /// <summary>
        /// Use only when absolutely necessary to ensure the power grid has a 100% actual state.
        /// </summary>
        public static void ServerForceRebuildPowerGrid(ILogicObject powerGrid)
        {
            ServerRebuildPowerGrid(powerGrid);
            ServerRebuildPowerGridNowIfDirty(powerGrid, PowerGrid.GetPublicState(powerGrid));
        }

        public static void ServerRecalculateGridCapacity(
            ILogicObject powerGrid,
            PowerGridPublicState state)
        {
            var newCapacity = BaseCapacity;
            foreach (var storageObject in state.ServerCacheStorage)
            {
                var protoStorage = (IProtoObjectElectricityStorage)storageObject.ProtoGameObject;
                newCapacity += protoStorage.ElectricityCapacity;

                var storagePrivateState = storageObject.GetPrivateState<ObjectPowerGridPrivateState>();
                storagePrivateState.PowerGrid = powerGrid;
            }

            state.NumberStorages = (ushort)state.ServerCacheStorage.Count;

            var oldCapacity = state.ElectricityCapacity;
            if (newCapacity == oldCapacity)
            {
                return;
            }

            state.ElectricityCapacity = newCapacity;

            var newAmount = state.ElectricityAmount;
            if (double.IsNaN(newAmount)
                || double.IsInfinity(newAmount))
            {
                Logger.Error($"Incorrect energy amount in power grid, will reset to 0: {newAmount} {powerGrid}");
                newAmount = 0;
            }

            newAmount = Math.Min(newAmount, newCapacity);

            if (oldCapacity > newCapacity)
            {
                // Capacity decreased.
                // Decrease the electricity amount such that % of amount (related to total capacity) remains the same.
                // Basically, the idea is that electricity is spread out equally across all the storage devices.
                newAmount *= newCapacity / oldCapacity;
            }

            state.ElectricityAmount = newAmount;

            Logger.Info($"Power grid capacity changed: {powerGrid} - {state.ElectricityCapacity} EU");
        }

        public static void ServerRegisterGrid(ILogicObject powerGrid)
        {
            Logger.Info("Server register power grid: " + powerGrid);
            ServerRebuildPowerGrid(powerGrid);
        }

        public static void ServerUnregisterGrid(ILogicObject powerGrid)
        {
            Logger.Info("Server unregister power grid: " + powerGrid);
            var state = PowerGrid.GetPublicState(powerGrid);
            ServerCutPower(powerGrid, state);
            ServerStopGenerators(state);
        }

        public static void ServerUpdateGrid(
            ILogicObject powerGrid,
            PowerGridPublicState state,
            double deltaTime)
        {
            ServerRebuildPowerGridNowIfDirty(powerGrid, state);

            double efficiencyMultiplier = state.EfficiencyMultiplier,
                   consumptionCurrent = 0,
                   consumptionTotalDemand = 0,
                   productionCurrent = 0,
                   productionTotalAvailable = 0;

            ushort numberConsumersActive = 0,
                   numberConsumersOutage = 0,
                   numberProducersActive = 0;

            var gridElectricityAmountPercent = SharedGetPowerGridChargePercent(state);
            var gridElectricityAmountPercentByte = (byte)gridElectricityAmountPercent;

            // process consumers
            foreach (var consumerObject in state.ServerCacheConsumers)
            {
                var protoConsumer = (IProtoObjectElectricityConsumer)consumerObject.ProtoGameObject;
                var energy = protoConsumer.ElectricityConsumptionPerSecondWhenActive;
                consumptionTotalDemand += energy;

                var consumerPrivateState = consumerObject.GetPrivateState<IObjectElectricityStructurePrivateState>();
                consumerPrivateState.PowerGridChargePercent = gridElectricityAmountPercentByte;

                var consumerPublicState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerPublicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnIdle)
                {
                    if (gridElectricityAmountPercent >= 1
                        && gridElectricityAmountPercent >= consumerPrivateState.ElectricityThresholds.StartupPercent)
                    {
                        // consumer startup threshold reached
                        consumerPublicState.ElectricityConsumerState = ElectricityConsumerState.PowerOnActive;
                    }
                    else
                    {
                        numberConsumersOutage++;
                    }
                }

                if (consumerPublicState.ElectricityConsumerState != ElectricityConsumerState.PowerOnActive)
                {
                    continue;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (gridElectricityAmountPercent == 0
                    || gridElectricityAmountPercent < consumerPrivateState.ElectricityThresholds.ShutdownPercent)
                {
                    // consumer shutdown threshold reached
                    consumerPublicState.ElectricityConsumerState = ElectricityConsumerState.PowerOnIdle;
                    numberConsumersOutage++;
                    continue;
                }

                if (protoConsumer is IProtoObjectElectricityConsumerWithCustomRate protoConsumerCustom)
                {
                    var rate = protoConsumerCustom.SharedGetCurrentElectricityConsumptionRate(consumerObject);
                    if (rate <= 0)
                    {
                        continue;
                    }

                    energy *= MathHelper.Clamp(rate, 0, 1);
                }

                consumptionCurrent += energy;
                numberConsumersActive++;
            }

            // process producers
            {
                if (state.ServerNeedToSortCacheProducers)
                {
                    ServerSortProducers(state.ServerCacheProducers);
                    state.ServerNeedToSortCacheProducers = false;
                }

                if (state.ServerNeedToSortCacheConsumers)
                {
                    ServerSortConsumers(state.ServerCacheConsumers);
                    state.ServerNeedToSortCacheConsumers = false;
                }

                foreach (var producerObject in state.ServerCacheProducers)
                {
                    var protoProducer = (IProtoObjectElectricityProducer)producerObject.ProtoGameObject;
                    protoProducer.SharedGetElectricityProduction(producerObject,
                                                                 out var energyCurrent,
                                                                 out var energyMax);

                    energyCurrent *= efficiencyMultiplier;
                    energyMax *= efficiencyMultiplier;

                    productionTotalAvailable += energyMax;

                    if (energyCurrent > 0)
                    {
                        productionCurrent += energyCurrent;
                        numberProducersActive++;
                    }

                    var producerPrivateState =
                        producerObject.GetPrivateState<IObjectElectricityStructurePrivateState>();
                    producerPrivateState.PowerGridChargePercent = gridElectricityAmountPercentByte;

                    var producerPublicState = producerObject.GetPublicState<IObjectElectricityProducerPublicState>();
                    if (protoProducer.IsGeneratorAlwaysOn)
                    {
                        producerPublicState.ElectricityProducerState = ElectricityProducerState.PowerOnActive;
                    }
                    else
                    {
                        switch (producerPublicState.ElectricityProducerState)
                        {
                            case ElectricityProducerState.PowerOff:
                                continue;

                            case ElectricityProducerState.PowerOnIdle:
                            {
                                // this generator is idle (not generating energy)
                                if (gridElectricityAmountPercent
                                    <= producerPrivateState.ElectricityThresholds.StartupPercent)
                                {
                                    // startup threshold is reached
                                    producerPublicState.ElectricityProducerState =
                                        ElectricityProducerState.PowerOnActive;
                                    numberProducersActive++;
                                }

                                break;
                            }

                            case ElectricityProducerState.PowerOnActive:
                            {
                                // this generator is active
                                if (gridElectricityAmountPercent
                                    >= producerPrivateState.ElectricityThresholds.ShutdownPercent)
                                {
                                    // shutdown threshold is reached
                                    producerPublicState.ElectricityProducerState = ElectricityProducerState.PowerOnIdle;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            var amount = state.ElectricityAmount
                         + (productionCurrent - consumptionCurrent) * deltaTime;

            if (amount < 0)
            {
                // Not sufficient energy to satisfy all currently active consumers.
                amount = productionCurrent * deltaTime;
                // (assume state.ElectricityAmount is zero so if there
                // is no generation going on the power grid will have 0 charge)

                // disable all consumers
                numberConsumersOutage = 0;
                foreach (var consumerObject in state.ServerCacheConsumers)
                {
                    var consumerPublicState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                    switch (consumerPublicState.ElectricityConsumerState)
                    {
                        case ElectricityConsumerState.PowerOff:
                            continue;

                        case ElectricityConsumerState.PowerOnActive:
                            consumerPublicState.ElectricityConsumerState = ElectricityConsumerState.PowerOnIdle;
                            break;
                    }

                    numberConsumersOutage++;
                }
            }

            if (double.IsNaN(amount)
                || double.IsInfinity(amount))
            {
                Logger.Error($"Incorrect energy amount in power grid, will reset to 0: {amount} {powerGrid}");
                amount = 0;
            }

            state.ElectricityAmount = MathHelper.Clamp(amount, 0, state.ElectricityCapacity);
            state.ElectricityConsumptionCurrent = consumptionCurrent;
            state.ElectricityConsumptionTotalDemand = consumptionTotalDemand;
            state.ElectricityProductionCurrent = productionCurrent;
            state.ElectricityProductionTotalAvailable = productionTotalAvailable;

            state.NumberConsumers = (ushort)state.ServerCacheConsumers.Count;
            state.NumberConsumersActive = numberConsumersActive;
            state.NumberProducers = (ushort)state.ServerCacheProducers.Count;
            state.NumberProducersActive = numberProducersActive;
        }

        protected override void PrepareSystem()
        {
            ConstructionPlacementSystem.ServerStructureBuilt += ServerStructureBuiltHandler;
            ServerStructuresManager.StructureRemoved += ServerStructureRemovedHandler;
            LandClaimSystem.ServerAreasGroupCreated += ServerLandClaimsGroupCreatedHandler;
            LandClaimSystem.ServerAreasGroupDestroyed += ServerLandClaimsGroupDestroyedHandler;
            LandClaimSystem.ServerAreasGroupChanged += ServerLandClaimsGroupChangedHandler;
            LandClaimSystem.ServerObjectLandClaimDestroyed += ServerObjectLandClaimDestroyedHandler;
            LandClaimSystem.ServerBaseMerge += ServerBaseMergeHandler;
            ConstructionRelocationSystem.ServerStructureBeforeRelocating += ServerStructureRelocatingOrRelocatedHandler;
            ConstructionRelocationSystem.ServerStructureRelocated += ServerStructureRelocatingOrRelocatedHandler;

            if (IsClient)
            {
                InteractableWorldObjectHelper.ClientMenuCreated += ClientMenuCreatedHandler;
                return;
            }

            // if server
            var allProtoGenerators = FindProtoEntities<IProtoObjectElectricityProducer>();
            try
            {
                allProtoGenerators = allProtoGenerators.TopologicalSort(
                    GetProducerDependencies,
                    throwOnCycle: true);

                for (var index = 0; index < allProtoGenerators.Count; index++)
                {
                    var protoObjectElectricityProducer = allProtoGenerators[index];
                    protoObjectElectricityProducer.GenerationOrder = index;
                }
            }
            catch (TopologicalSortExtensions.ExceptionCircularDependency<IProtoObjectElectricityProducer> exception)
            {
                var badItem = exception.Item;
                var circularDependencies = allProtoGenerators
                                           .ToDictionary(e => e, GetProducerDependencies)
                                           .Where(p => p.Value.Contains(badItem)).Select(p => p.Key);
                throw new Exception(
                    string.Format("There are circular dependencies for {1} and {2}."
                                  + "{0}Please check {3} on these classes.",
                                  Environment.NewLine,
                                  badItem,
                                  circularDependencies.GetJoinedString(),
                                  nameof(PrepareOrderAttribute)));
            }

            IEnumerable<IProtoObjectElectricityProducer> GetProducerDependencies(IProtoObjectElectricityProducer e)
                => ElectricityProductionOrderAttribute.GetDependencies(e, allProtoGenerators);
        }

        private static void ClientMenuCreatedHandler(IWorldObject worldObject, BaseUserControlWithWindow menu)
        {
            if (worldObject is IStaticWorldObject staticWorldObject)
            {
                ClientTryInjectPowerSwitchUI(staticWorldObject, menu);
            }
        }

        private static void ServerBaseMergeHandler(ILogicObject areasGroupFrom, ILogicObject areasGroupTo)
        {
            var fromState = LandClaimAreasGroup.GetPrivateState(areasGroupFrom);
            var toState = LandClaimAreasGroup.GetPrivateState(areasGroupTo);
            PowerGrid.ServerOnPowerGridMerged(fromState.PowerGrid, toState.PowerGrid);
        }

        /// <summary>
        /// The bigger the area (in terms of connected land claims number) the less efficient
        /// power distribution is to make energy requirements more steep for larger bases.
        /// Basically generators generate slightly less power each cycle.
        /// Efficiency cannot fall below 50%.
        /// Efficiency formula: 100%-5%*(claims-1).
        /// </summary>
        private static double ServerCalculateEfficiencyMultiplier(ILogicObject areasGroup, out byte landClaimsCount)
        {
            landClaimsCount = (byte)Math.Min(LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                                .ServerLandClaimsAreas
                                                                .Count,
                                             byte.MaxValue);

            var result = 1
                         - ((PowerGridEfficiencyDropPercentPerExtraLandClaim / 100.0)
                            * (landClaimsCount - 1));
            return Math.Max(result, PowerGridMinEfficiencyPercents / 100.0);
        }

        private static void ServerCutPower(
            ILogicObject powerGrid,
            PowerGridPublicState state)
        {
            Logger.Info($"Power cut: {powerGrid}");

            foreach (var consumerObject in state.ServerCacheConsumers)
            {
                var consumerPrivateState = consumerObject.GetPrivateState<IObjectElectricityStructurePrivateState>();
                consumerPrivateState.PowerGridChargePercent = 0;

                var consumerPublicState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerPublicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive)
                {
                    consumerPublicState.ElectricityConsumerState = ElectricityConsumerState.PowerOnIdle;
                }
            }
        }

        private static void ServerLandClaimsGroupChangedHandler(
            ILogicObject area,
            [CanBeNull] ILogicObject areasGroupFrom,
            [CanBeNull] ILogicObject areasGroupTo)
        {
            TryRebuild(areasGroupFrom);
            TryRebuild(areasGroupTo);

            void TryRebuild(ILogicObject areasGroup)
            {
                if (areasGroup is not null)
                {
                    ServerRebuildPowerGrid(SharedGetPowerGrid(areasGroup));
                }
            }
        }

        private static void ServerLandClaimsGroupCreatedHandler(ILogicObject areasGroup)
        {
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var grid = ServerWorld.CreateLogicObject<PowerGrid>();
            areasGroupPrivateState.PowerGrid = grid;
            PowerGrid.GetPublicState(grid).ServerAreasGroup = areasGroup;
            Logger.Info($"Power grid created: {grid} for {areasGroup}");
        }

        private static void ServerLandClaimsGroupDestroyedHandler(ILogicObject areasGroup)
        {
            var grid = SharedGetPowerGrid(areasGroup);
            // don't reset it in state as it will be used during the electricity amount merge
            //areasGroupPrivateState.PowerGrid = null;

            var powerGridPublicState = PowerGrid.GetPublicState(grid);
            foreach (var storageObject in powerGridPublicState.ServerCacheStorage)
            {
                var storagePrivateState = storageObject.GetPrivateState<ObjectPowerGridPrivateState>();
                if (ReferenceEquals(storagePrivateState.PowerGrid, grid))
                {
                    storagePrivateState.PowerGrid = null;
                }
            }

            ServerCutPower(grid,
                           powerGridPublicState);
            ServerStopGenerators(powerGridPublicState);

            Logger.Info($"Power grid destroyed: {grid} for {areasGroup}");
            ServerWorld.DestroyObject(grid);
        }

        private static void ServerObjectLandClaimDestroyedHandler(
            IStaticWorldObject landClaimStructure,
            RectangleInt areaBounds,
            LandClaimAreaPublicState areaPublicState,
            bool isDestroyedByPlayers,
            bool isDeconstructed)
        {
            var powerGrid = SharedGetPowerGrid(areaPublicState.LandClaimAreasGroup);
            if (powerGrid is null)
            {
                Logger.Error("Power grid is null");
                return;
            }

            Logger.Info(
                $"Cutting off the power for objects inside the destroyed land claim area: {landClaimStructure} {areaBounds}");
            var consumersInArea =
                ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityConsumer>(areaBounds);

            foreach (var consumerObject in consumersInArea)
            {
                var consumerState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive)
                {
                    consumerState.ElectricityConsumerState = ElectricityConsumerState.PowerOnIdle;
                }
            }

            var producersInArea =
                ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityProducer>(areaBounds);
            foreach (var producerObject in producersInArea)
            {
                var producerState = producerObject.GetPublicState<IObjectElectricityProducerPublicState>();
                // TODO: we need a way to restore power to power producers too, consider introducing PowerOffOutage
                producerState.ElectricityProducerState = ElectricityProducerState.PowerOff;
            }

            ServerRebuildPowerGrid(powerGrid);
        }

        private static void ServerRebuildPowerGrid(ILogicObject powerGrid)
        {
            var state = PowerGrid.GetPublicState(powerGrid);
            if (state.ServerIsDirty)
            {
                // already scheduled rebuild
                return;
            }

            state.ServerIsDirty = true;
            Logger.Info("Server scheduled recalculation of the power grid: " + powerGrid);
            ServerTimersSystem.AddAction(0,
                                         () => ServerRebuildPowerGridNowIfDirty(powerGrid, state));
        }

        private static void ServerRebuildPowerGridNowIfDirty(
            ILogicObject powerGrid,
            PowerGridPublicState state)
        {
            if (!state.ServerIsDirty)
            {
                return;
            }

            state.ServerIsDirty = false;

            var areasGroup = state.ServerAreasGroup;
            if (areasGroup is null
                || areasGroup.IsDestroyed)
            {
                return;
            }

            var boundingBox = LandClaimSystem.SharedGetLandClaimGroupBoundingArea(areasGroup);
            if (boundingBox == default)
            {
                return;
            }

            Logger.Info("Server recalculating power grid: " + powerGrid);
            using var areasBounds = Api.Shared.GetTempList<RectangleInt>();
            foreach (var area in LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                    .ServerLandClaimsAreas)
            {
                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area);
                areasBounds.Add(bounds);
            }

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityConsumer>(boundingBox),
                     state.ServerCacheConsumers);
            state.ServerNeedToSortCacheConsumers = true;

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityProducer>(boundingBox),
                     state.ServerCacheProducers);
            state.ServerNeedToSortCacheProducers = true;

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityStorage>(boundingBox),
                     state.ServerCacheStorage);

            // remove consumers with 0 max consumption and fix the invalid electricity thresholds
            var consumers = state.ServerCacheConsumers;
            for (var index = 0; index < consumers.Count; index++)
            {
                var objectConsumer = consumers[index];
                var protoConsumer = (IProtoObjectElectricityConsumer)objectConsumer.ProtoGameObject;
                if (protoConsumer.ElectricityConsumptionPerSecondWhenActive <= 0)
                {
                    consumers.RemoveAt(index--);
                    continue;
                }

                var privateState = objectConsumer.GetPrivateState<IObjectElectricityStructurePrivateState>();
                if (privateState.ElectricityThresholds.IsInvalid)
                {
                    privateState.ElectricityThresholds = protoConsumer.DefaultConsumerElectricityThresholds;
                }
            }

            foreach (var objectProducer in state.ServerCacheProducers)
            {
                var privateState = objectProducer.GetPrivateState<IObjectElectricityStructurePrivateState>();
                if (privateState.ElectricityThresholds.IsInvalid)
                {
                    var protoConsumer = (IProtoObjectElectricityProducer)objectProducer.ProtoGameObject;
                    privateState.ElectricityThresholds = protoConsumer.DefaultGenerationElectricityThresholds;
                }
            }

            ServerRecalculateGridCapacity(powerGrid, state);

            state.EfficiencyMultiplier = ServerCalculateEfficiencyMultiplier(areasGroup, out var landClaimsCount);
            state.NumberLandClaims = landClaimsCount;

            void FillList(
                IEnumerable<IStaticWorldObject> sourceEnumeration,
                List<IStaticWorldObject> resultList)
            {
                resultList.Clear();

                foreach (var worldObject in sourceEnumeration)
                {
                    var position = worldObject.TilePosition;
                    foreach (var areaBounds in areasBounds.AsList())
                    {
                        if (!areaBounds.Contains(position))
                        {
                            continue;
                        }

                        // it's important to use "if not contains" check here
                        // as the enumeration could provide the same object several times
                        resultList.AddIfNotContains(worldObject);
                        break;
                    }
                }
            }
        }

        private static SetPowerModeResult ServerSetConsumerPowerMode(IStaticWorldObject worldObject, bool isOn)
        {
            var consumerPublicState = worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
            var newPowerState = isOn
                                    ? ElectricityConsumerState.PowerOnActive
                                    : ElectricityConsumerState.PowerOff;

            if (newPowerState == consumerPublicState.ElectricityConsumerState)
            {
                return SetPowerModeResult.Success;
            }

            var powerGrid = SharedGetPowerGrid(worldObject);
            if (powerGrid is null && isOn)
            {
                return SetPowerModeResult.NoPowerGridExist;
            }

            var isNotEnoughPower = false;
            if (newPowerState == ElectricityConsumerState.PowerOnActive)
            {
                var powerGridState = PowerGrid.GetPublicState(powerGrid);
                var gridElectricityAmountPercent = (byte)SharedGetPowerGridChargePercent(powerGridState);
                if (gridElectricityAmountPercent == 0)
                {
                    isNotEnoughPower = true;
                }

                var consumerPrivateState = worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>();
                if (gridElectricityAmountPercent <= consumerPrivateState.ElectricityThresholds.ShutdownPercent)
                {
                    // consumer shutdown threshold reached
                    newPowerState = ElectricityConsumerState.PowerOnIdle;
                    if (newPowerState == consumerPublicState.ElectricityConsumerState)
                    {
                        return isNotEnoughPower
                                   ? SetPowerModeResult.NotEnoughPower
                                   : SetPowerModeResult.Success;
                    }
                }
            }

            consumerPublicState.ElectricityConsumerState = newPowerState;
            Logger.Info($"Changed consumer power mode: {worldObject} to {newPowerState}");

            return isNotEnoughPower
                       ? SetPowerModeResult.NotEnoughPower
                       : SetPowerModeResult.Success;
        }

        private static SetPowerModeResult ServerSetProducerPowerMode(
            IStaticWorldObject worldObject,
            ElectricityProducerState newPowerState)
        {
            var publicState = worldObject.GetPublicState<IObjectElectricityProducerPublicState>();

            if (newPowerState == publicState.ElectricityProducerState
                || (newPowerState == ElectricityProducerState.PowerOnIdle
                    && publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive))
            {
                return SetPowerModeResult.Success;
            }

            var powerGrid = SharedGetPowerGrid(worldObject);
            if (powerGrid is null
                && newPowerState != ElectricityProducerState.PowerOff)
            {
                return SetPowerModeResult.NoPowerGridExist;
            }

            if (newPowerState == ElectricityProducerState.PowerOnActive)
            {
                var state = PowerGrid.GetPublicState(powerGrid);
                if (state.ElectricityAmount >= state.ElectricityCapacity)
                {
                    // generation is not necessary now
                    newPowerState = ElectricityProducerState.PowerOnIdle;
                }
            }

            publicState.ElectricityProducerState = newPowerState;
            Logger.Info($"Changed producer power mode: {worldObject} to {newPowerState}");
            return SetPowerModeResult.Success;
        }

        private static void ServerSortConsumers(List<IStaticWorldObject> producers)
        {
            producers.Sort(ObjectConsumerOrderComparer.Instance);
        }

        private static void ServerSortProducers(List<IStaticWorldObject> producers)
        {
            producers.Sort(ObjectProducerOrderComparer.Instance);
        }

        private static void ServerStopGenerators(PowerGridPublicState gridPublicState)
        {
            foreach (var producer in gridPublicState.ServerCacheProducers)
            {
                var state = producer.GetPublicState<IObjectElectricityProducerPublicState>();
                state.ElectricityProducerState = ElectricityProducerState.PowerOff;
            }
        }

        private static void ServerStructureBuiltHandler(ICharacter character, IStaticWorldObject structure)
        {
            var protoStructure = structure.ProtoGameObject;
            var protoConsumer = protoStructure as IProtoObjectElectricityConsumer;
            var protoProducer = protoStructure as IProtoObjectElectricityProducer;

            if (protoProducer is not null
                || protoConsumer is not null)
            {
                var privateState = structure.GetPrivateState<IObjectElectricityStructurePrivateState>();
                privateState.ElectricityThresholds = protoProducer?.DefaultGenerationElectricityThresholds
                                                     ?? protoConsumer.DefaultConsumerElectricityThresholds;
            }

            if (protoProducer is not null)
            {
                var state = TryGetPowerGridState();
                if (state is null)
                {
                    return;
                }

                state.ServerCacheProducers.Add(structure);
                state.ServerNeedToSortCacheProducers = true;
                ServerSetProducerPowerMode(structure, ElectricityProducerState.PowerOnIdle);
            }

            if (protoConsumer is not null
                && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0)
            {
                var state = TryGetPowerGridState();
                if (state is null)
                {
                    return;
                }

                state.ServerCacheConsumers.Add(structure);
                state.ServerNeedToSortCacheConsumers = true;

                ServerSetConsumerPowerMode(structure, isOn: true);
            }

            if (protoStructure is IProtoObjectElectricityStorage)
            {
                var state = TryGetPowerGridState();
                if (state is null)
                {
                    return;
                }

                state.ServerCacheStorage.Add(structure);
                ServerRecalculateGridCapacity(powerGrid: (ILogicObject)state.GameObject, state);
            }

            PowerGridPublicState TryGetPowerGridState()
            {
                var powerGrid = SharedGetPowerGrid(structure);
                return powerGrid is null
                           ? null
                           : PowerGrid.GetPublicState(powerGrid);
            }
        }

        private static void ServerStructureRelocatingOrRelocatedHandler(
            ICharacter byCharacter,
            Vector2Ushort fromPosition,
            IStaticWorldObject worldObject)
        {
            var powerGrid = SharedGetPowerGrid(worldObject);
            if (powerGrid is not null)
            {
                ServerRebuildPowerGrid(powerGrid);
            }
        }

        private static void ServerStructureRemovedHandler(IStaticWorldObject structure)
        {
            var protoStructure = structure.ProtoGameObject;
            if (protoStructure is IProtoObjectElectricityConsumer protoConsumer
                && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0)
            {
                var state = TryGetPowerGridState();
                state?.ServerCacheConsumers.Remove(structure);
            }

            if (protoStructure is IProtoObjectElectricityProducer)
            {
                var state = TryGetPowerGridState();
                state?.ServerCacheProducers.Remove(structure);
            }

            if (protoStructure is IProtoObjectElectricityStorage)
            {
                var state = TryGetPowerGridState();
                if (state is not null)
                {
                    state.ServerCacheStorage.Remove(structure);
                    ServerRecalculateGridCapacity(powerGrid: (ILogicObject)state.GameObject, state);
                }
            }

            PowerGridPublicState TryGetPowerGridState()
            {
                var powerGrid = SharedGetPowerGrid(structure);
                return powerGrid is null
                           ? null
                           : PowerGrid.GetPublicState(powerGrid);
            }
        }

        private static ILogicObject SharedGetPowerGrid(IStaticWorldObject structure)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(structure.OccupiedTile.Position);
            if (areasGroup is null)
            {
                return null;
            }

            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            return areasGroupPrivateState.PowerGrid;
        }

        private static ILogicObject SharedGetPowerGrid(ILogicObject areasGroup)
        {
            return LandClaimAreasGroup.GetPrivateState(areasGroup).PowerGrid;
        }

        private static double SharedGetPowerGridChargePercent(PowerGridPublicState powerGridPublicState)
        {
            var fraction = powerGridPublicState.ElectricityAmount / powerGridPublicState.ElectricityCapacity;
            fraction = MathHelper.Clamp(fraction, 0, 1);
            return 100 * fraction;
        }

        private void ServerRemote_SetElectricityThresholds(
            IStaticWorldObject worldObject,
            ElectricityThresholdsPreset thresholds)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                return;
            }

            thresholds = thresholds.Normalize(
                isProducer: worldObject.ProtoGameObject is IProtoObjectElectricityProducer);

            switch (worldObject.ProtoGameObject)
            {
                case IProtoObjectElectricityProducer:
                    worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>()
                               .ElectricityThresholds = thresholds;
                    return;

                case IProtoObjectElectricityConsumer protoConsumer
                    when protoConsumer.ElectricityConsumptionPerSecondWhenActive <= 0:
                    Logger.Error("This consumer doesn't consume any energy (defined consumption is 0): " + worldObject);
                    return;

                case IProtoObjectElectricityConsumer:
                    worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>()
                               .ElectricityThresholds = thresholds;
                    return;

                default:
                    Logger.Error("This structure is not a power producer or consumer: " + worldObject);
                    return;
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 1, keyArgIndex: 0)]
        private SetPowerModeResult ServerRemote_SetPowerMode(IStaticWorldObject worldObject, bool isOn)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                return SetPowerModeResult.CannotInteractWithObject;
            }

            switch (worldObject.ProtoStaticWorldObject)
            {
                case IProtoObjectElectricityProducer:
                    // producer
                    return ServerSetProducerPowerMode(worldObject,
                                                      isOn
                                                          ? ElectricityProducerState.PowerOnActive
                                                          : ElectricityProducerState.PowerOff);
                // consumer
                case IProtoObjectElectricityConsumer protoConsumer
                    when protoConsumer.ElectricityConsumptionPerSecondWhenActive <= 0:
                    Logger.Error("This consumer doesn't consume any energy (defined consumption is 0): " + worldObject);
                    return SetPowerModeResult.CannotInteractWithObject;

                case IProtoObjectElectricityConsumer:
                    return ServerSetConsumerPowerMode(worldObject, isOn);

                default:
                    return SetPowerModeResult.CannotInteractWithObject;
            }
        }

        private class ObjectConsumerOrderComparer : IComparer<IStaticWorldObject>
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly ObjectConsumerOrderComparer Instance = new();

            private ObjectConsumerOrderComparer()
            {
            }

            public int Compare(IStaticWorldObject x, IStaticWorldObject y)
            {
                var sx = x.GetPrivateState<IObjectElectricityStructurePrivateState>();
                var sy = y.GetPrivateState<IObjectElectricityStructurePrivateState>();

                return sx.ElectricityThresholds.StartupPercent
                         .CompareTo(sy.ElectricityThresholds.StartupPercent);
            }
        }

        private class ObjectProducerOrderComparer : IComparer<IStaticWorldObject>
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly ObjectProducerOrderComparer Instance = new();

            private ObjectProducerOrderComparer()
            {
            }

            public int Compare(IStaticWorldObject x, IStaticWorldObject y)
            {
                var sx = x.GetPrivateState<IObjectElectricityStructurePrivateState>();
                var sy = y.GetPrivateState<IObjectElectricityStructurePrivateState>();

                var result = sx.ElectricityThresholds.StartupPercent
                               .CompareTo(sy.ElectricityThresholds.StartupPercent);

                if (result != 0)
                {
                    // the reverse value is intentional here
                    return -result;
                }

                var protoA = (IProtoObjectElectricityProducer)x.ProtoStaticWorldObject;
                var protoB = (IProtoObjectElectricityProducer)y.ProtoStaticWorldObject;
                return protoA.GenerationOrder
                             .CompareTo(protoB.GenerationOrder);
            }
        }
    }
}