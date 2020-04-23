namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Completionist
{
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ConsoleCompletionistListPlayer : BaseConsoleCommand
    {
        public override string Description => "Lists all completionist entries for player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "completionist.list";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var completionistData = PlayerCharacter.GetPrivateState(player).CompletionistData;
            var sb = new StringBuilder(player.ToString())
                .AppendLine("Unlocked completionist entries:");

            Append("Food", completionistData.ListFood);
            Append("Mobs", completionistData.ListMobs);
            Append("Loot", completionistData.ListLoot);

            return sb.ToString();

            void Append(string key, NetworkSyncList<DataEntryCompletionist> entries)
            {
                sb.Append(key)
                  .AppendLine(":");
                foreach (var entry in entries)
                {
                    sb.Append("* ")
                      .AppendLine(entry.Prototype.ShortId);
                }

                sb.AppendLine();
            }
        }
    }
}