namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class TaskDiscoverTeleportsAll : BasePlayerTaskWithDefaultState
    {
        /// <summary>
        /// It's not localizable as we're using this strictly for an achievement.
        /// </summary>
        [NotLocalizable]
        public const string DefaultDescription = "Discover all teleports";

        private TaskDiscoverTeleportsAll()
            : base(DefaultDescription)
        {
        }

        public override bool IsReversible => false;

        public static TaskDiscoverTeleportsAll Require()
        {
            return new();
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var totalTeleportsCount = TeleportsSystem.ServerAllTeleports.Count;
            return totalTeleportsCount > 0
                   && (TeleportsSystem.ServerGetDiscoveredTeleports(character).Count
                       >= totalTeleportsCount);
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
            var activeContext = this.GetActiveContext(character, out _);
            activeContext?.Refresh();
        }
    }
}