namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.StatusEffects
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleStatusEffectsReset : BaseConsoleCommand
    {
        public override string Description => "Remove all status effects.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "statusEffects.reset";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            player.ServerRemoveAllStatusEffects();
            return null;
        }
    }
}