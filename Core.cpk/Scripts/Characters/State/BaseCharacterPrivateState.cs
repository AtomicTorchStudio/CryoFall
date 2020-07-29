namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseCharacterPrivateState : BasePrivateState
    {
        [TempOnly]
        public ushort? ContainerEquipmentLastStateHash { get; set; }

        [TempOnly]
        [SubscribableProperty]
        public FinalStatsCache FinalStatsCache { get; set; }

        [SyncToClient]
        public NetworkSyncList<ILogicObject> StatusEffects { get; private set; }

        [TempOnly]
        public WeaponState WeaponState { get; } = new WeaponState();

        public void EnsureEverythingCreated()
        {
            if (this.StatusEffects is null)
            {
                // TODO: actually, we don't need this to be instantiated for mobs by default
                this.StatusEffects = new NetworkSyncList<ILogicObject>();
            }
        }

        /// <summary>
        /// Invalidates character's final stats cache (will be re-calculated on the next ClientUpdate/ServerUpdate call).
        /// </summary>
        public void SetFinalStatsCacheIsDirty()
        {
            if (this.FinalStatsCache.IsDirty)
            {
                return;
            }

            Api.Logger.Important(this.GameObject + " final stats cache marked as dirty");
            this.FinalStatsCache = this.FinalStatsCache.AsDirty();
        }
    }
}