namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementVisitTile : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Visit: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private RequirementVisitTile(IProtoTile protoTile, string description)
            : base(description)
        {
            this.ProtoTile = protoTile;
        }

        public override bool IsReversible => false;

        public IProtoTile ProtoTile { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ProtoTile.Name);

        public static RequirementVisitTile Require<TProtoTile>(string description = null)
            where TProtoTile : class, IProtoTile, new()
        {
            var protoTile = Api.GetProtoEntity<TProtoTile>();
            return new RequirementVisitTile(protoTile, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
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