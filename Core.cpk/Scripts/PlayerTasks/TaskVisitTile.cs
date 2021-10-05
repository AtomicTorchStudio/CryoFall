namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskVisitTile : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionFormat = "Visit: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private TaskVisitTile(IProtoTile protoTile, string description)
            : base(description)
        {
            this.ProtoTile = protoTile;
        }

        public override bool IsReversible => false;

        public IProtoTile ProtoTile { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ProtoTile.Name);

        public static TaskVisitTile Require<TProtoTile>(string description = null)
            where TProtoTile : class, IProtoTile, new()
        {
            var protoTile = Api.GetProtoEntity<TProtoTile>();
            return new TaskVisitTile(protoTile, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.ProtoTile.EditorIconTexture;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            if (!character.ServerIsOnline
                || character.TilePosition == CharacterDespawnSystem.ServerGetServiceAreaPosition())
            {
                return false;
            }

            return character.Tile.ProtoTile == this.ProtoTile;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                this.serverUpdater = new ServerWrappedTriggerTimeInterval(
                    this.ServerRefreshAllActiveContexts,
                    TimeSpan.FromSeconds(1),
                    "QuestRequirement.VisitTile");
            }
            else
            {
                this.serverUpdater.Dispose();
                this.serverUpdater = null;
            }
        }
    }
}