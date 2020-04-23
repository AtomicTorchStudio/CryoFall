namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IPlayerTask
    {
        string Description { get; }

        ITextureResource Icon { get; }

        bool IsReversible { get; }

        IProtoLogicObject TaskTarget { get; set; }

        PlayerTaskState CreateState();

        bool IsStateTypeMatch(PlayerTaskState state);

        void ServerRefreshIsCompleted(ICharacter character, PlayerTaskState state);

        void ServerRegisterOrUnregisterContext(ServerPlayerActiveTask context);
    }
}