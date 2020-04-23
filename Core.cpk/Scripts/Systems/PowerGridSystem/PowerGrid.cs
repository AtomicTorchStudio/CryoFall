namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    [PrepareOrder(typeof(LandClaimAreasGroup))]
    public class PowerGrid
        : ProtoGameObject<ILogicObject,
              EmptyPrivateState,
              PowerGridPublicState,
              EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        public override string Name => CoreStrings.TitlePowerGrid;

        public override double ServerUpdateIntervalSeconds => 1;

        public static void ServerOnPowerGridBroken(ILogicObject fromPowerGrid, List<ILogicObject> toPowerGrids)
        {
            var fromPowerGridState = GetPublicState(fromPowerGrid);
            var totalElectricityAmount = fromPowerGridState.ElectricityAmount;
            var totalElectricityStorageCapacity = fromPowerGridState.ElectricityCapacity - PowerGridSystem.BaseCapacity;

            var totalPowerGridsCount = 1 + toPowerGrids.Count;
            Logger.Info(
                $"Power grid broken. Electricity split: {totalElectricityAmount:F0} to {totalPowerGridsCount} power grids from {fromPowerGrid}");

            fromPowerGridState.ElectricityAmount = CalculateElectricityPart(fromPowerGrid, fromPowerGridState);

            foreach (var toPowerGrid in toPowerGrids)
            {
                var toState = GetPublicState(toPowerGrid);
                // we're using += here as the new power grid might be an already existing grid from another base
                toState.ElectricityAmount += CalculateElectricityPart(toPowerGrid, toState);
            }

            double CalculateElectricityPart(ILogicObject powerGrid, PowerGridPublicState powerGridState)
            {
                // force recalculate grid capacity
                // (it could (and for the "fromPowerGrid", will) apply penalty which we will ignore)
                PowerGridSystem.ServerForceRebuildPowerGrid(powerGrid);

                var powerGridCapacity = powerGridState.ElectricityCapacity - PowerGridSystem.BaseCapacity;
                var electricityFraction = powerGridCapacity / totalElectricityStorageCapacity;
                var electricityPart = totalElectricityAmount * electricityFraction;
                Logger.Info($"Electricity share part: {electricityPart:F0} for {powerGrid}");
                return electricityPart;
            }
        }

        public static void ServerOnPowerGridMerged(ILogicObject fromPowerGrid, ILogicObject toPowerGrid)
        {
            var fromState = GetPublicState(fromPowerGrid);
            var toState = GetPublicState(toPowerGrid);

            Logger.Info(
                $"Power grid merged. Electricity merged: {fromState.ElectricityAmount:F0}+{toState.ElectricityAmount:F0} with 50% penalty. {fromPowerGrid} and {toPowerGrid}");

            // Combine charge from both power grids.
            // (due to base size limit of 3x3 it's no longer reasonable to exploit this
            // by merging small generation base with the primary base)
            toState.ElectricityAmount = fromState.ElectricityAmount + toState.ElectricityAmount;

            Logger.Info($"Electricity merge result: {toState.ElectricityAmount:F0} in {toPowerGrid}");
        }

        public override void ServerOnDestroy(ILogicObject gameObject)
        {
            PowerGridSystem.ServerUnregisterGrid(gameObject);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            data.PublicState.ServerInitState();
            PowerGridSystem.ServerRegisterGrid(data.GameObject);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            PowerGridSystem.ServerUpdateGrid(data.GameObject,
                                             data.PublicState,
                                             data.DeltaTime);
        }
    }
}