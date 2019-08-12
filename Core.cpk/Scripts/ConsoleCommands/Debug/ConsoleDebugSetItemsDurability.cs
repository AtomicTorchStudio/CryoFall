// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ConsoleDebugSetItemsDurability : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies durability of all items in player's inventory/equipment/hotbar to match the required durability percent (provided as value from 0 to 100).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setItemsDurability";

        public string Execute(
            double durabilityPercent = 100,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            var durabilityFraction = MathHelper.Clamp(durabilityPercent / 100, min: 0, max: 1);
            var containers = new AggregatedItemsContainers(character, includeEquipmentContainer: true);

            foreach (var container in containers)
            {
                foreach (var item in Api.Shared.WrapInTempList(container.Items))
                {
                    if (item.ProtoItem is IProtoItemWithDurablity protoItemWithDurablity)
                    {
                        var privateState = item.GetPrivateState<IItemWithDurabilityPrivateState>();
                        var durability = protoItemWithDurablity.DurabilityMax * durabilityFraction;
                        privateState.DurabilityCurrent = (uint)(Math.Round(durability, MidpointRounding.AwayFromZero));
                    }
                }
            }

            return $"Durability of all items modified to {(durabilityFraction * 100)}%";
        }
    }
}