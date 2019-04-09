namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.StatusEffects
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleStatusEffectAddAll : BaseConsoleCommand
    {
        public override string Description =>
            "Adds all possible status effects (some of them will be immediately remowed, however, as they cannot exist together).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "statusEffects.addAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            foreach (var protoStatusEffect in Api.FindProtoEntities<IProtoStatusEffect>())
            {
                player.ServerAddStatusEffect(protoStatusEffect, 1);
            }

            return null;
        }
    }
}