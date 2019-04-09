namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestAdd : BaseConsoleCommand
    {
        public override string Description => "Add quest to a player (even if prerequisites are not satisfied).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.add";

        public string Execute(IProtoQuest quest, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            quests.ServerTryAddQuest(quest, isUnlocked: true);
            return null;
        }
    }
}