namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.StatusEffects
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleStatusEffectRemove : BaseConsoleCommand
    {
        public override string Description => "Removes specified status effect from a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "statusEffects.remove";

        public string Execute(IProtoStatusEffect statusEffect, [CurrentCharacterIfNull] ICharacter player)
        {
            player.ServerRemoveStatusEffect(statusEffect);
            return null;
        }
    }
}