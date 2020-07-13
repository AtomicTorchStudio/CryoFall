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

    public class ConsoleDebugSetItemsFreshness : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies freshness of all items in player's inventory/equipment/hotbar to match the required freshness percent (provided as value from 0 to 100).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setItemsFreshness";

        public string Execute(
            double freshnessPercent = 100,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            var freshnessFraction = MathHelper.Clamp(freshnessPercent / 100, min: 0, max: 1);
            var containers = new AggregatedItemsContainers(character, includeEquipmentContainer: true);

            foreach (var container in containers)
            {
                foreach (var item in Api.Shared.WrapInTempList(container.Items).EnumerateAndDispose())
                {
                    if (item.ProtoItem is IProtoItemWithFreshness protoItemWithFreshness
                        && protoItemWithFreshness.FreshnessMaxValue > 0)
                    {
                        var privateState = item.GetPrivateState<IItemWithFreshnessPrivateState>();
                        var durability = protoItemWithFreshness.FreshnessMaxValue * freshnessFraction;
                        privateState.FreshnessCurrent = (uint)Math.Round(durability, MidpointRounding.AwayFromZero);
                    }
                }
            }

            return $"Freshness of all items modified to {freshnessFraction * 100}%";
        }
    }
}