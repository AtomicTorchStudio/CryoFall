namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.StatusEffects
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleStatusEffectAdd : BaseConsoleCommand
    {
        public override string Description => "Adds specified status effect to a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "statusEffects.add";

        public string Execute(
            IProtoStatusEffect statusEffect,
            double intensityToAdd = 1.0,
            [CurrentCharacterIfNull] ICharacter player = null)
        {
            player.ServerAddStatusEffect(statusEffect, intensityToAdd);
            return null;
        }
    }
}