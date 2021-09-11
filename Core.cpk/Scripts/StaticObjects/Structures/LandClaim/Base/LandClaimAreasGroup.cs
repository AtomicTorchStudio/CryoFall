namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    [PrepareOrder(typeof(LandClaimArea))]
    public class LandClaimAreasGroup
        : ProtoGameObject<ILogicObject,
              LandClaimAreasGroupPrivateState,
              LandClaimAreasGroupPublicState,
              EmptyClientState>,
          IProtoLogicObjectWithInteraction
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        [NotLocalizable]
        public override string Name => "Land claim area group";

        public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

        public static void ServerOnBaseMerged(ILogicObject areasGroupFrom, ILogicObject areasGroupTo)
        {
            var fromState = GetPrivateState(areasGroupFrom);
            var toState = GetPrivateState(areasGroupTo);

            var fromContainer = fromState.ItemsContainerSafeStorage;
            if (fromContainer.OccupiedSlotsCount != 0)
            {
                var toContainer = toState.ItemsContainerSafeStorage;
                Server.Items.TryMoveAllItems(fromContainer, toContainer, onlyToExistingStacks: true);

                var slotsNeeded = fromContainer.OccupiedSlotsCount;
                if (slotsNeeded > 0)
                {
                    Server.Items.SetSlotsCount(toContainer,
                                               Math.Max(RatePvPSafeStorageCapacity.SharedValue,
                                                        (byte)(toContainer.OccupiedSlotsCount + slotsNeeded)));
                    Server.Items.TryMoveAllItems(fromContainer, toContainer, onlyToExistingStacks: false);
                }

                Logger.Important($"Bases merged: moved all safe storage items from {fromContainer} to {toContainer}");
            }
        }

        public static void ServerOnGroupChanged(
            ILogicObject area,
            ILogicObject areasGroupFrom,
            ILogicObject areasGroupTo)
        {
            var stateFrom = GetPublicState(areasGroupFrom);
            var stateTo = GetPublicState(areasGroupTo);

            if (stateFrom.LastRaidTime.HasValue
                && stateFrom.LastRaidTime.Value > (stateTo.LastRaidTime ?? 0))
            {
                stateTo.LastRaidTime = stateFrom.LastRaidTime;
                //Logger.Dev($"Copied last raid time from {areasGroupFrom} to {areasGroupTo}");
            }
            else
            {
                stateFrom.LastRaidTime = stateTo.LastRaidTime;
                //Logger.Dev($"Copied last raid time from {areasGroupTo} to {areasGroupFrom}");
            }
        }

        public bool SharedCanInteract(ICharacter character, ILogicObject logicObject, bool writeToLog)
        {
            if (IsClient)
            {
                // we cannot perform this check on the client side yet
                return true;
            }

            var privateState = GetPrivateState(logicObject);
            var world = Server.World;
            foreach (var landClaim in privateState.ServerLandClaimsAreas)
            {
                if (world.IsInPrivateScope(landClaim, character))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void PrepareProto()
        {
            LandClaimSystem.ServerBaseBroken += ServerBaseBrokenHandler;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            var itemsContainerSafeStorage = data.PrivateState.ItemsContainerSafeStorage;
            var safeStorageCapacity = RatePvPSafeStorageCapacity.SharedValue;

            if (itemsContainerSafeStorage is null)
            {
                itemsContainerSafeStorage = Server.Items.CreateContainer<ItemsContainerLandClaimSafeStorage>(
                    owner: data.GameObject,
                    slotsCount: safeStorageCapacity);

                data.PrivateState.ItemsContainerSafeStorage = itemsContainerSafeStorage;
            }
            else
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainerSafeStorage,
                                           slotsCount: Math.Max(itemsContainerSafeStorage.SlotsCount,
                                                                safeStorageCapacity));
            }
        }

        private static void ServerBaseBrokenHandler(ILogicObject areasGroup, List<ILogicObject> newAreaGroups)
        {
            var fromPowerGrid = GetPrivateState(areasGroup).PowerGrid;
            var toPowerGrids = newAreaGroups.Select(g => GetPrivateState(g).PowerGrid).ToList();
            PowerGrid.ServerOnPowerGridBroken(fromPowerGrid, toPowerGrids);
        }
    }
}