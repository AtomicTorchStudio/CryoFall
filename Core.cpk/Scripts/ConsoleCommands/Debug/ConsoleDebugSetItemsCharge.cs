// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ConsoleDebugSetItemsCharge : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies charge/fuel amount of all items in player's inventory/equipment/hotbar to match the required charge percent (provided as value from 0 to 100).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setItemsCharge";

        public string Execute(
            double chargePercent = 100,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            var chargeFraction = MathHelper.Clamp(chargePercent / 100, min: 0, max: 1);
            var containers = new AggregatedItemsContainers(character, includeEquipmentContainer: true);

            foreach (var container in containers)
            {
                foreach (var item in Api.Shared.WrapInTempList(container.Items))
                {
                    switch (item.ProtoItem)
                    {
                        case IProtoItemPowerBank protoItemPowerBank:
                        {
                            var itemPrivateState = item.GetPrivateState<ItemPowerBankPrivateState>();
                            var capacity = protoItemPowerBank.EnergyCapacity;
                            itemPrivateState.EnergyCharge = capacity * chargeFraction;
                            break;
                        }

                        case IProtoItemWithFuel protoItemWithFuel:
                        {
                            var privateState = item.GetPrivateState<ItemWithFuelPrivateState>();
                            var fuel = protoItemWithFuel.ItemFuelConfig.FuelCapacity * chargeFraction;
                            privateState.FuelAmount = (uint)Math.Round(fuel, MidpointRounding.AwayFromZero);
                            protoItemWithFuel.ItemFuelConfig.SharedOnRefilled(item,
                                                                              privateState.FuelAmount,
                                                                              serverNotifyClients: true);
                            break;
                        }
                    }
                }
            }

            return $"Charge/fuel amount of all items modified to {chargeFraction * 100}%";
        }
    }
}