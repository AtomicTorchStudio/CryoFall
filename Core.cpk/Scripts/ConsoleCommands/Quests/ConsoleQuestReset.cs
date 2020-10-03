// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestReset : BaseConsoleCommand
    {
        public override string Description => "Reset quest to a player (if exist).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.reset";

        public string Execute(IProtoQuest quest, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            if (quests.SharedFindQuestEntry(quest, out var isUnlocked) is null)
            {
                return "Quest entry not found";
            }

            quests.ServerTryRemoveQuest(quest);
            quests.ServerTryAddQuest(quest, isUnlocked);
            return "Successfully reset the quest entry";
        }
    }
}