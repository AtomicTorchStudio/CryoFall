namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
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

            PowerGrid.ServerOnPowerGridMerged(fromState.PowerGrid, toState.PowerGrid);

            var fromContainer = fromState.ItemsContainer;
            if (fromContainer.OccupiedSlotsCount != 0)
            {
                var toContainer = toState.ItemsContainer;
                Server.Items.TryMoveAllItems(fromContainer, toContainer, onlyToExistingStacks: true);

                var slotsNeeded = fromContainer.OccupiedSlotsCount;
                if (slotsNeeded > 0)
                {
                    Server.Items.SetSlotsCount(toContainer,
                                               Math.Max(ItemsContainerLandClaimSafeStorage.ServerSafeItemsSlotsCapacity,
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

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var itemsContainer = data.PrivateState.ItemsContainer;
            var safeStorageCapacity = ItemsContainerLandClaimSafeStorage.ServerSafeItemsSlotsCapacity;

            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer,
                                           slotsCount: Math.Max(
                                               itemsContainer.SlotsCount,
                                               safeStorageCapacity));
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerLandClaimSafeStorage>(
                owner: data.GameObject,
                slotsCount: safeStorageCapacity);

            data.PrivateState.ItemsContainer = itemsContainer;
        }

        public static void ServerOnBaseBroken(ILogicObject areasGroup, List<ILogicObject> newGroups)
        {
            var fromPowerGrid = GetPrivateState(areasGroup).PowerGrid;
            var toPowerGrids = newGroups.Select(g => GetPrivateState(g).PowerGrid).ToList();
            PowerGrid.ServerOnPowerGridBroken(fromPowerGrid, toPowerGrids);
        }
    }
}