namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
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
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class PowerGridSystem : ProtoSystem<PowerGridSystem>
    {
        public const double BaseCapacity = 10000.0;

        // If the charge drops below 90% in the power grid, the required power producers (generators) will be automatically activated (to ensure the power generation surplus).
        public const double PowerProductionChargeThreshold = 0.9;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

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

        public override string Name => "Power grid system";

        public static void ClientInitializeConsumerOrProducer(IStaticWorldObject worldObject)
        {
            if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer protoConsumer
                && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0
                || worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer protoProducer)
            {
                ObjectPowerStateOverlay.CreateFor(worldObject);
            }
        }

        public static async void ClientRestorePower(PowerGridPublicState state)
        {
            var grid = (ILogicObject)state.GameObject;
            var result = await Instance.CallServer(_ => _.ServerRemote_RestorePower(grid));
            if (result == SetPowerModeResult.Success)
            {
                return;
            }

            NotificationSystem.ClientShowNotification(result.GetDescription(),
                                                      color: NotificationColor.Bad);
        }

        public static async Task<bool> ClientSetPowerMode(bool isOn, IStaticWorldObject worldObject)
        {
            var result = await Instance.CallServer(_ => _.ServerRemote_SetPowerMode(isOn, worldObject));
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
            if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer protoConsumer
                && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0
                || worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer)
            {
                // consumer or producer object
            }
            else
            {
                return;
            }

            var firstChild = (FrameworkElement)VisualTreeHelper.GetChild(window.Window, 0);
            var grid = firstChild.FindName<Grid>("ContentChromeGrid");

            var canvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            var powerSwitchControl = PowerSwitchControl.Create(worldObject);
            canvas.Children.Add(powerSwitchControl);
            Canvas.SetLeft(powerSwitchControl, 10);

            grid.Children.Add(canvas);
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
            Logger.Important(
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

            Logger.Important($"Power grid capacity changed: {powerGrid} - {state.ElectricityCapacity} EU");
        }

        public static void ServerRegisterGrid(ILogicObject powerGrid)
        {
            Logger.Important("Server register power grid: " + powerGrid);
            ServerRebuildPowerGrid(powerGrid);
        }

        public static void ServerUnregisterGrid(ILogicObject powerGrid)
        {
            Logger.Important("Server unregister power grid: " + powerGrid);
            var state = PowerGrid.GetPublicState(powerGrid);
            ServerCutPower(powerGrid, state, keepCriticalConsumers: false);
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
                   consumptionCurrentCriticalOnly = 0,
                   consumptionTotalDemand = 0,
                   consumptionTotalDemandCriticalOnly = 0,
                   productionCurrent = 0,
                   productionTotalAvailable = 0;

            ushort numberConsumersActive = 0,
                   numberConsumersOutage = 0,
                   numberProducersActive = 0;

            // process consumers
            foreach (var consumerObject in state.ServerCacheConsumers)
            {
                var protoConsumer = (IProtoObjectElectricityConsumer)consumerObject.ProtoGameObject;
                var energy = protoConsumer.ElectricityConsumptionPerSecondWhenActive;
                consumptionTotalDemand += energy;

                var isCriticalConsumer = protoConsumer is IProtoObjectElectricityConsumerCritical;
                if (isCriticalConsumer)
                {
                    consumptionTotalDemandCriticalOnly += energy;
                }

                var consumerState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerState.ElectricityConsumerState == ElectricityConsumerState.PowerOffOutage)
                {
                    numberConsumersOutage++;
                    continue;
                }

                if (consumerState.ElectricityConsumerState != ElectricityConsumerState.PowerOn)
                {
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

                if (isCriticalConsumer)
                {
                    consumptionCurrentCriticalOnly += energy;
                }
            }

            // process producers
            {
                var isPowerGridHasFullCharge = state.ElectricityAmount >= state.ElectricityCapacity;
                var isPowerGridChargeBelowThreshold = (state.ElectricityAmount / state.ElectricityCapacity)
                                                      < PowerProductionChargeThreshold;

                // power demand is used only to handle generators idle/active state
                var powerDemand = consumptionCurrent;
                if (state.ServerNeedToSortCacheProducers)
                {
                    ServerSortProducers(state.ServerCacheProducers);
                    state.ServerNeedToSortCacheProducers = false;
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

                    var productionCurrentWithoutThisProducer = productionCurrent;
                    if (energyCurrent > 0)
                    {
                        productionCurrent += energyCurrent;
                        numberProducersActive++;
                    }

                    var publicState = producerObject.GetPublicState<IObjectElectricityProducerPublicState>();
                    if (publicState.ElectricityProducerState == ElectricityProducerState.PowerOff)
                    {
                        continue;
                    }

                    // refresh power active/idle state for generator
                    if (publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive
                        && isPowerGridHasFullCharge
                        && (productionCurrent > powerDemand
                            || (powerDemand == 0
                                && productionCurrent == powerDemand)))
                    {
                        // this generator is not necessary now - go idle
                        publicState.ElectricityProducerState = ElectricityProducerState.PowerOnIdle;
                        //Logger.Dev("Producer is not required now and was deactivated: "
                        //           + producerObject);
                    }
                    else if (publicState.ElectricityProducerState == ElectricityProducerState.PowerOnIdle)
                    {
                        // this generator is idle (not generating energy)
                        if (!isPowerGridChargeBelowThreshold)
                        {
                            // no need to turn it on as threshold is not reached
                            continue;
                        }

                        if (productionCurrentWithoutThisProducer >= powerDemand
                            && numberProducersActive > 0)
                        {
                            // no need to turn it on as there is a power generation surplus
                            // and more than a single generator is active
                            continue;
                        }

                        //Logger.Dev("Production is below the generation threshold, producer activated: "
                        //           + producerObject);
                        publicState.ElectricityProducerState = ElectricityProducerState.PowerOnActive;
                        // power demand is used only to handle generators idle/active state
                        // assume the just enabled generator will instantly fill the power demand
                        // (the correct value will be calculated on the next tick)
                        powerDemand -= energyMax;
                        numberProducersActive++;
                    }
                }
            }

            var amount = state.ElectricityAmount
                         + (productionCurrent - consumptionCurrent) * deltaTime;

            if (amount < 0)
            {
                // Not sufficient energy to satisfy all currently active consumers.
                // Try calculate the amount for the case when only critical consumption is active.
                amount = state.ElectricityAmount
                         + (productionCurrent - consumptionCurrentCriticalOnly) * deltaTime;

                if (amount > 0)
                {
                    // Have sufficient energy to satisfy all currently active critical consumers.
                    ServerCutPower(powerGrid, state, keepCriticalConsumers: true);
                }
                else
                {
                    // Not sufficient energy to satisfy all currently active critical consumers.
                    ServerCutPower(powerGrid, state, keepCriticalConsumers: false);

                    amount = state.ElectricityAmount
                             + productionCurrent * deltaTime;
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
            state.ElectricityConsumptionTotalDemandCriticalOnly = consumptionTotalDemandCriticalOnly;
            state.ElectricityProductionCurrent = productionCurrent;
            state.ElectricityProductionTotalAvailable = productionTotalAvailable;

            state.NumberConsumers = (ushort)state.ServerCacheConsumers.Count;
            state.NumberConsumersActive = numberConsumersActive;
            state.NumberProducers = (ushort)state.ServerCacheProducers.Count;
            state.NumberProducersActive = numberProducersActive;

            state.IsBlackout = numberConsumersOutage > 0;
        }

        protected override void PrepareSystem()
        {
            ConstructionPlacementSystem.ServerStructureBuilt += ServerStructureBuiltHandler;
            ServerStructuresManager.StructureDestroyed += ServerStructureDestroyedHandler;
            LandClaimSystem.ServerAreasGroupCreated += ServerLandClaimsGroupCreatedHandler;
            LandClaimSystem.ServerAreasGroupDestroyed += ServerLandClaimsGroupDestroyedHandler;
            LandClaimSystem.ServerAreasGroupChanged += ServerLandClaimsGroupChangedHandler;

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

            var result = 1 - 0.05 * (landClaimsCount - 1);
            return Math.Max(result, 0.5);
        }

        private static void ServerCutPower(
            ILogicObject powerGrid,
            PowerGridPublicState state,
            bool keepCriticalConsumers)
        {
            Logger.Important($"Power cut: {powerGrid}. Keep critical consumers active: {keepCriticalConsumers}");

            foreach (var consumerObject in state.ServerCacheConsumers)
            {
                var consumerState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerState.ElectricityConsumerState != ElectricityConsumerState.PowerOn)
                {
                    continue;
                }

                if (!keepCriticalConsumers
                    || !(consumerObject.ProtoGameObject is IProtoObjectElectricityConsumerCritical))
                {
                    consumerState.ElectricityConsumerState = ElectricityConsumerState.PowerOffOutage;
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
                if (areasGroup != null)
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
            Logger.Important($"Power grid created: {grid} for {areasGroup}");
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
                           powerGridPublicState,
                           keepCriticalConsumers: false);
            ServerStopGenerators(powerGridPublicState);

            Logger.Important($"Power grid destroyed: {grid} for {areasGroup}");
            ServerWorld.DestroyObject(grid);
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
            Logger.Important("Server scheduled recalculation of the power grid: " + powerGrid);
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
            if (areasGroup == null
                || areasGroup.IsDestroyed)
            {
                return;
            }

            var boundingBox = LandClaimSystem.SharedGetLandClaimGroupsBoundingArea(areasGroup);
            if (boundingBox == default)
            {
                return;
            }

            Logger.Important("Server recalculating power grid: " + powerGrid);
            using var areasBounds = Api.Shared.GetTempList<RectangleInt>();
            foreach (var area in LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                    .ServerLandClaimsAreas)
            {
                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area);
                areasBounds.Add(bounds);
            }

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityConsumer>(boundingBox),
                     state.ServerCacheConsumers);

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityProducer>(boundingBox),
                     state.ServerCacheProducers);
            state.ServerNeedToSortCacheProducers = true;

            FillList(ServerWorld.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectElectricityStorage>(boundingBox),
                     state.ServerCacheStorage);

            // remove consumers with 0 max consumption
            var consumers = state.ServerCacheConsumers;
            for (var index = 0; index < consumers.Count; index++)
            {
                var consumer = consumers[index];
                if (((IProtoObjectElectricityConsumer)consumer.ProtoGameObject)
                    .ElectricityConsumptionPerSecondWhenActive
                    <= 0)
                {
                    consumers.RemoveAt(index--);
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
            var protoConsumer = (IProtoObjectElectricityConsumer)worldObject.ProtoGameObject;
            var publicState = worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
            var newPowerState = isOn
                                    ? ElectricityConsumerState.PowerOn
                                    : ElectricityConsumerState.PowerOff;

            if (newPowerState == publicState.ElectricityConsumerState)
            {
                return SetPowerModeResult.Success;
            }

            var powerGrid = SharedGetPowerGrid(worldObject);
            if (powerGrid == null)
            {
                return SetPowerModeResult.NoPowerGridExist;
            }

            if (newPowerState == ElectricityConsumerState.PowerOn)
            {
                var powerGridState = PowerGrid.GetPublicState(powerGrid);
                var electricitySurplus = powerGridState.ElectricityAmount
                                         + powerGridState.ElectricityProductionCurrent
                                         - powerGridState.ElectricityConsumptionCurrent;
                if (electricitySurplus < protoConsumer.ElectricityConsumptionPerSecondWhenActive)
                {
                    return SetPowerModeResult.NotEnoughPower;
                }
            }

            publicState.ElectricityConsumerState = newPowerState;
            Logger.Important($"Changed consumer power mode: {worldObject} to {newPowerState}");
            return SetPowerModeResult.Success;
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
            if (powerGrid == null
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
            Logger.Important($"Changed producer power mode: {worldObject} to {newPowerState}");
            return SetPowerModeResult.Success;
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
            if (protoStructure is IProtoObjectElectricityConsumer protoConsumer
                && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0)
            {
                var state = TryGetPowerGridState();
                if (state == null)
                {
                    return;
                }

                state.ServerCacheConsumers.Add(structure);

                if (protoConsumer is IProtoObjectElectricityConsumerCritical)
                {
                    // do not enable new critical (defense) consumer (such as psionic projector)
                    ServerSetConsumerPowerMode(structure, isOn: false);
                }
                else
                {
                    // auto-enable new regular consumer
                    ServerSetConsumerPowerMode(structure, isOn: true);
                }
            }

            if (protoStructure is IProtoObjectElectricityProducer)
            {
                var state = TryGetPowerGridState();
                if (state == null)
                {
                    return;
                }

                state.ServerCacheProducers.Add(structure);
                state.ServerNeedToSortCacheProducers = true;
                ServerSetProducerPowerMode(structure, ElectricityProducerState.PowerOnIdle);
            }

            if (protoStructure is IProtoObjectElectricityStorage)
            {
                var state = TryGetPowerGridState();
                if (state == null)
                {
                    return;
                }

                state.ServerCacheStorage.Add(structure);
                ServerRecalculateGridCapacity(powerGrid: (ILogicObject)state.GameObject, state);
            }

            PowerGridPublicState TryGetPowerGridState()
            {
                var powerGrid = SharedGetPowerGrid(structure);
                return powerGrid == null
                           ? null
                           : PowerGrid.GetPublicState(powerGrid);
            }
        }

        private static void ServerStructureDestroyedHandler(IStaticWorldObject structure)
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
                if (state != null)
                {
                    state.ServerCacheStorage.Remove(structure);
                    ServerRecalculateGridCapacity(powerGrid: (ILogicObject)state.GameObject, state);
                }
            }

            PowerGridPublicState TryGetPowerGridState()
            {
                var powerGrid = SharedGetPowerGrid(structure);
                return powerGrid == null
                           ? null
                           : PowerGrid.GetPublicState(powerGrid);
            }
        }

        private static ILogicObject SharedGetPowerGrid(IStaticWorldObject structure)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(structure.OccupiedTile.Position);
            if (areasGroup == null)
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

        private SetPowerModeResult ServerRemote_RestorePower(ILogicObject powerGrid)
        {
            if (!ServerWorld.IsInPublicScope(powerGrid, ServerRemoteContext.Character))
            {
                return SetPowerModeResult.CannotInteractWithObject;
            }

            var powerGridState = PowerGrid.GetPublicState(powerGrid);
            var electricitySurplus = powerGridState.ElectricityAmount
                                     + powerGridState.ElectricityProductionCurrent
                                     - powerGridState.ElectricityConsumptionCurrent;

            // first pass - check all consumers in power outage state
            foreach (var consumerObject in powerGridState.ServerCacheConsumers)
            {
                var consumerState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerState.ElectricityConsumerState != ElectricityConsumerState.PowerOffOutage)
                {
                    continue;
                }

                var protoConsumer = (IProtoObjectElectricityConsumer)consumerObject.ProtoGameObject;
                electricitySurplus -= protoConsumer.ElectricityConsumptionPerSecondWhenActive;
                if (electricitySurplus < 0)
                {
                    return SetPowerModeResult.NotEnoughPower;
                }
            }

            // second pass - enable all consumers in power outage state
            foreach (var consumerObject in powerGridState.ServerCacheConsumers)
            {
                var consumerState = consumerObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                if (consumerState.ElectricityConsumerState != ElectricityConsumerState.PowerOffOutage)
                {
                    continue;
                }

                consumerState.ElectricityConsumerState = ElectricityConsumerState.PowerOn;
            }

            return SetPowerModeResult.Success;
        }

        private SetPowerModeResult ServerRemote_SetPowerMode(bool isOn, IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                return SetPowerModeResult.CannotInteractWithObject;
            }

            if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer protoProducer)
            {
                // producer
                return ServerSetProducerPowerMode(worldObject,
                                                  isOn
                                                      ? ElectricityProducerState.PowerOnActive
                                                      : ElectricityProducerState.PowerOff);
            }

            if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer protoConsumer)
            {
                // consumer
                if (protoConsumer.ElectricityConsumptionPerSecondWhenActive <= 0)
                {
                    Logger.Error("This consumer doesn't consume any energy (defined consumption is 0): " + worldObject);
                    return SetPowerModeResult.CannotInteractWithObject;
                }

                return ServerSetConsumerPowerMode(worldObject, isOn);
            }

            return SetPowerModeResult.CannotInteractWithObject;
        }

        private class ObjectProducerOrderComparer : IComparer<IStaticWorldObject>
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly ObjectProducerOrderComparer Instance = new ObjectProducerOrderComparer();

            private ObjectProducerOrderComparer()
            {
            }

            public int Compare(IStaticWorldObject x, IStaticWorldObject y)
            {
                var protoA = (IProtoObjectElectricityProducer)x.ProtoStaticWorldObject;
                var protoB = (IProtoObjectElectricityProducer)y.ProtoStaticWorldObject;
                return protoA.GenerationOrder
                             .CompareTo(protoB.GenerationOrder);
            }
        }
    }
}