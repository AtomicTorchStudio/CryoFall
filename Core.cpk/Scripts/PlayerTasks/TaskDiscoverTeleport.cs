namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskDiscoverTeleport : BasePlayerTaskWithCount<PlayerTaskStateWithCount>
    {
        public const string DescriptionFormat = "Discover: {0}";

        private TaskDiscoverTeleport(
            ProtoObjectTeleport protoObjectTeleport,
            ushort count,
            string description)
            : base(count,
                   description)
        {
            this.ProtoObjectTeleport = protoObjectTeleport;
        }

        public override bool IsReversible => false;

        public ProtoObjectTeleport ProtoObjectTeleport { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ProtoObjectTeleport.Name);

        public static TaskDiscoverTeleport Require<TProtoObjectTeleport>(
            ushort count = 1,
            string description = null)
            where TProtoObjectTeleport : ProtoObjectTeleport, new()
        {
            var protoObjectTeleport = Api.GetProtoEntity<TProtoObjectTeleport>();
            return Require(protoObjectTeleport, count, description);
        }

        public static TaskDiscoverTeleport Require(
            ProtoObjectTeleport protoObjectTeleport,
            ushort count = 1,
            string description = null)
        {
            return new(protoObjectTeleport, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.ProtoObjectTeleport.Icon;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                TeleportsSystem.ServerTeleportDiscovered += this.ServerTeleportDiscoveredHandler;
            }
            else
            {
                TeleportsSystem.ServerTeleportDiscovered -= this.ServerTeleportDiscoveredHandler;
            }
        }

        private void ServerTeleportDiscoveredHandler(
            IStaticWorldObject staticWorldObject,
            ICharacter character)
        {
            if (staticWorldObject.ProtoGameObject != this.ProtoObjectTeleport)
            {
                // different teleport type
                return;
            }

            var activeContext = this.GetActiveContext(character, out var state);
            if (activeContext is null)
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            activeContext.Refresh();
        }
    }
}