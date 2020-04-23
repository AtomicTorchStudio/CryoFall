namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Completionist
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleCompletionistAddAll : BaseConsoleCommand
    {
        public override string Description => "Add all completionist entries to player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "completionist.addAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var completionistData = PlayerCharacter.GetPrivateState(player).CompletionistData;
            foreach (var protoFood in CompletionistSystem.CompletionistAllFood)
            {
                completionistData.ServerOnItemUsed(protoFood);
                completionistData.ServerTryClaimReward(protoFood);
            }

            foreach (var protoMob in CompletionistSystem.CompletionistAllMobs)
            {
                completionistData.ServerOnMobKilled(protoMob);
                completionistData.ServerTryClaimReward(protoMob);
            }

            foreach (var protoLoot in CompletionistSystem.CompletionistAllLoot)
            {
                completionistData.ServerOnLootReceived(protoLoot);
                completionistData.ServerTryClaimReward(protoLoot);
            }

            return null;
        }
    }
}