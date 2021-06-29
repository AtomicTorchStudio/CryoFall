namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectMineralMeteorite
        : ProtoObjectMineral
          <EmptyPrivateState,
              ObjectMineralMeteoritePublicState,
              DefaultMineralClientState>,
          IProtoObjectEventEntry
    {
        private readonly double serverRateLootCountMultiplier;

        protected ProtoObjectMineralMeteorite()
        {
            if (IsServer)
            {
                this.serverRateLootCountMultiplier = ServerRates.Get(
                    "DropListItemsCountMultiplier." + this.ShortId,
                    defaultValue: 1.0,
                    @"This rate determines the item droplist multiplier for loot in " + this.Name + ".");
            }
        }

        public override bool IsAllowDroneMining => false;

        public override bool IsAllowQuickMining => false;

        public abstract double ServerCooldownDuration { get; }

        protected override bool CanFlipSprite => true;

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            ClientMeteoriteTooltipHelper.Refresh(data.GameObject, isObserving);
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            base.PrepareTileRequirements(tileRequirements);
            tileRequirements.Add(LandClaimSystem.ValidatorFreeLandEvenForServer)
                            .Add(ConstructionTileRequirements.ValidatorNotRestrictedAreaEvenForServer)
                            .Add(ConstructionTileRequirements.ValidatorTileNotRestrictingConstructionEvenForServer);
        }

        protected override double ServerGetDropListProbabilityMultiplier(IStaticWorldObject mineralObject)
        {
            // compensate for the general server items drop rate
            // but apply a separate rate
            return this.serverRateLootCountMultiplier
                   / DropItemsList.DropListItemsCountMultiplier;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                data.PublicState.CooldownUntilServerTime
                    = Server.Game.FrameTime + this.ServerCooldownDuration;
            }
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            var serverTime = IsServer
                                 ? Server.Game.FrameTime
                                 : Client.CurrentGame.ServerFrameTimeApproximated;

            if (serverTime < GetPublicState(targetObject).CooldownUntilServerTime)
            {
                // too hot for mining - no damage to it
                if (IsClient
                    && weaponCache.ProtoWeapon is IProtoItemToolMining)
                {
                    NotificationSystem.ClientShowNotification(CoreStrings.Meteorite_CooldownMessage_TooHotForMining,
                                                              color: NotificationColor.Bad,
                                                              icon: this.Icon);
                }

                if (IsServer
                    && weaponCache.ProtoWeapon is IProtoItemWeaponMelee
                    && !weaponCache.Character.IsNpc)
                {
                    weaponCache.Character.ServerAddStatusEffect<StatusEffectHeat>(intensity: 0.5);
                }

                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                return 0;
            }

            // meteorite cooldown finished
            if (NewbieProtectionSystem.SharedIsNewbie(weaponCache.Character))
            {
                // don't allow mining meteorite while under newbie protection
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
    }
}