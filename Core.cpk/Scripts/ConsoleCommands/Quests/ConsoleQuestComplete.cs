namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestComplete : BaseConsoleCommand
    {
        public override string Description =>
            "Complete a specific active quest for player (even if prerequisites are not satisfied).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.complete";

        public string Execute(IProtoQuest quest, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            QuestsSystem.ServerCompleteQuest(quests, quest, ignoreRequirements: true);
            return null;
        }
    }
}