namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoObjectBossLoot
        : ProtoObjectMineral
            <ProtoObjectBossLoot.PrivateState,
                StaticObjectPublicState,
                DefaultMineralClientState>
    {
        // The remains destruction will be postponed on this duration
        // if it cannot be destroy because there are characters observing it.
        private static readonly double DestructionTimeoutPostponeSeconds
            = TimeSpan.FromMinutes(2).TotalSeconds;

        // The remains will destroy after this duration if there are no characters observing it.
        private static readonly double DestructionTimeoutSeconds
            = Math.Max(30 * 60, WorldObjectClaimDuration.BossLoot);

        public override bool IsAllowDroneMining => false;

        public override bool IsAllowQuickMining => false;

        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // reset the destroy timer (even if object is already initialized (e.g. loading a savegame))
            data.PrivateState.DestroyAtTime = Server.Game.FrameTime + DestructionTimeoutSeconds;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            if (Api.IsEditor)
            {
                // do not destroy by timeout in Editor
                return;
            }

            var privateState = data.PrivateState;
            var timeNow = Server.Game.FrameTime;

            // Destroy Pragmium node if the timeout is exceeded
            // and there is no Pragmium Source node nearby
            // and there are no player characters observing it.
            if (timeNow < privateState.DestroyAtTime)
            {
                return;
            }

            // should destroy because timed out
            var worldObject = data.GameObject;
            if (Server.World.IsObservedByAnyPlayer(worldObject))
            {
                // cannot destroy - there are players observing it
                privateState.DestroyAtTime = timeNow + DestructionTimeoutPostponeSeconds;
                return;
            }

            Server.World.DestroyObject(worldObject);
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (NewbieProtectionSystem.SharedIsNewbie(weaponCache.Character))
            {
                // don't allow mining a boss loot while under newbie protection
                if (IsClient)
                {
                    NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
                }

                obstacleBlockDamageCoef = 0;
                return 0;
            }

            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        public class PrivateState : BasePrivateState
        {
            public double DestroyAtTime { get; set; }
        }
    }
}