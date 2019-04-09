namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestCompleteAllActive : BaseConsoleCommand
    {
        public override string Description =>
            "Complete all active quests to a player (even if prerequisites are not satisfied).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.completeAllActive";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            foreach (var questEntry in quests.Quests.ToList())
            {
                if (!questEntry.IsCompleted)
                {
                    QuestsSystem.ServerCompleteQuest(quests, questEntry.Quest, ignoreRequirements: true);
                }
            }

            return null;
        }
    }
}