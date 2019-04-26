// ReSharper disable CanExtractXamlLocalizableStringCSharp
namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetCreativeMode : BaseConsoleCommand
    {
        public override string Alias => "creative";

        public override string Description
            => "Toggles creative mode."
               + Environment.NewLine
               + "This mode allows you to build without using any resources also skipping the entire build phases. "
               + Environment.NewLine
               + "This mode is enabled by default in the Editor mode.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setCreativeMode";

        public string Execute(bool isEnabled, [CurrentCharacterIfNull] ICharacter character)
        {
            CreativeModeSystem.ServerSetCreativeMode(character, isEnabled);
            return null;
        }
    }
}