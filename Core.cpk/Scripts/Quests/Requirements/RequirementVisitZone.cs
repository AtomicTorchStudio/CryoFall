namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementVisitZone : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Visit: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private RequirementVisitZone(IProtoZone protoZone, string description)
            : base(description)
        {
            this.ProtoZone = protoZone;
        }

        public override bool IsReversible => false;

        public IProtoZone ProtoZone { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ProtoZone.Name);

        public static RequirementVisitZone Require<TProtoZone>(string description = null)
            where TProtoZone : class, IProtoZone, new()
        {
            var protoTile = Api.GetProtoEntity<TProtoZone>();
            return new RequirementVisitZone(protoTile, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            return this.ProtoZone
                       .ServerZoneInstance
                       .IsContainsPosition(character.TilePosition);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                this.serverUpdater = new ServerWrappedTriggerTimeInterval(
                    this.ServerRefreshAllActiveContexts,
                    TimeSpan.FromSeconds(1),
                    "QuestRequirement.VisitZone");
            }
            else
            {
                this.serverUpdater.Dispose();
                this.serverUpdater = null;
            }
        }
    }
}