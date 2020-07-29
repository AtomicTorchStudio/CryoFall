namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectGeneratorSolar
        : ProtoObjectGenerator
            <ObjectGeneratorPrivateState,
                ObjectGeneratorSolarPublicState,
                StaticObjectClientState>
    {
        public abstract byte PanelSlotsCount { get; }

        public static double SharedGetCurrentLightFraction()
        {
            var rate = TimeOfDaySystem.DayFraction * 4;
            rate = Math.Min(rate, 1);

            rate = Math.Round(rate,
                              digits: 1,
                              MidpointRounding.AwayFromZero);

            return rate;
        }

        public static double SharedGetElectricityProductionRate(IStaticWorldObject worldObjectGenerator)
        {
            if (!GetPublicState(worldObjectGenerator).IsActive)
            {
                return 0;
            }

            return SharedGetCurrentLightFraction();
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                GetPublicState(gameObject).PanelsContainer);
        }

        public override void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction)
        {
            maxProduction = 0;
            var panels = GetPublicState(worldObject).PanelsContainer.Items;
            foreach (var item in panels)
            {
                if (item.ProtoItem is IProtoItemSolarPanel panel)
                {
                    maxProduction += panel.ElectricityProductionPerSecond;
                }
            }

            if (maxProduction <= 0)
            {
                currentProduction = 0;
                return;
            }

            var rate = SharedGetElectricityProductionRate(worldObject);
            currentProduction = maxProduction * rate;
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowGeneratorSolar.Open(
                new ViewModelWindowGeneratorSolar(
                    data.GameObject,
                    data.PublicState));
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var publicState = data.PublicState;
            var itemsContainer = publicState.PanelsContainer;
            var itemsSlotsCount = this.PanelSlotsCount;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerGeneratorSolar>(
                owner: data.GameObject,
                slotsCount: itemsSlotsCount);

            publicState.PanelsContainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var publicState = data.PublicState;
            publicState.IsActive = publicState.ElectricityProducerState
                                   == ElectricityProducerState.PowerOnActive;
        }
    }
}