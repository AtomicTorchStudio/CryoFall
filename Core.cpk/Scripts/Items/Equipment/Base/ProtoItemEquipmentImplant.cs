namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterIdleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Item prototype for implants.
    /// </summary>
    public abstract class ProtoItemEquipmentImplant
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentImplant
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private double durabilityDecreasePerServerUpdate;

        protected ProtoItemEquipmentImplant()
        {
            this.Icon = new TextureResource("Items/Implants/" + this.GetType().Name);
        }

        public virtual ushort BiomaterialAmountRequiredToInstall => 25;

        public virtual ushort BiomaterialAmountRequiredToUninstall => 10;

        public override uint DurabilityMax => 6000;

        public sealed override EquipmentType EquipmentType => EquipmentType.Implant;

        public override double GroundIconScale => 0.85;

        public override ITextureResource Icon { get; }

        public override bool IsRepairable => false;

        /// <summary>
        /// Implant lifetime (applies only when implant is installed and the player is online).
        /// </summary>
        public virtual TimeSpan Lifetime => TimeSpan.FromDays(2);

        public override bool RequireEquipmentTexturesFemale => false;

        public override bool RequireEquipmentTexturesMale => false;

        // ten minutes, required for good granularity for durability degradation
        public sealed override double ServerUpdateIntervalSeconds => 10 * 60;

        protected override double DefenseMultiplier => 0;

        /// <summary>
        /// Determines how much durability the implant should loss
        /// (from 0.0 to 1.0, by default 0.02 (2% percents)).
        /// </summary>
        protected virtual double DurabilityFractionReduceOnDeath => 0.02;

        public override void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            if (!isEquipped)
            {
                // not installed implants always drop on death
                shouldDrop = true;
                return;
            }

            // installed implants never drop on death
            shouldDrop = false;

            // reduce implant's durability on death
            var fraction = MathHelper.Clamp(this.DurabilityFractionReduceOnDeath, 0, 1);
            var durabilityDelta = this.DurabilityMax * fraction;
            ItemDurabilitySystem.ServerModifyDurability(item,
                                                        delta: -durabilityDelta,
                                                        roundUp: false);
        }

        public sealed override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            base.ServerOnItemBrokeAndDestroyed(item, container, slotId);

            if (!(item.ProtoItem is ItemImplantBroken))
            {
                // place a broken implant to the released container slot
                Server.Items.CreateItem<ItemImplantBroken>(container, slotId: slotId);
            }
        }

        public override void ServerOnItemDamaged(IItem item, double damageApplied)
        {
            var owner = item.Container.OwnerAsCharacter;
            if (owner is not null)
            {
                damageApplied *= owner.SharedGetFinalStatMultiplier(StatName.ImplantDegradationFromDamageMultiplier);
            }

            ItemDurabilitySystem.ServerModifyDurability(item, delta: -(int)damageApplied);
        }

        protected sealed override void PrepareDefense(DefenseDescription defense)
        {
            // no defenses
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.ImplantApplication);
        }

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();

            if (this.DurabilityMax == 0
                || this.Lifetime.TotalSeconds <= 0)
            {
                this.durabilityDecreasePerServerUpdate = 0;
            }
            else
            {
                // pre-calculate durability decrease value per server update
                this.durabilityDecreasePerServerUpdate = this.ServerUpdateIntervalSeconds
                                                         * this.DurabilityMax
                                                         / this.Lifetime.TotalSeconds;

                var minGranularityRequired = 20;
                if (this.durabilityDecreasePerServerUpdate < minGranularityRequired)
                {
                    Logger.Error(
                        $"Too low degradation rate for {this.ShortId}: {this.durabilityDecreasePerServerUpdate} durability points per {this.ServerUpdateIntervalSeconds} seconds (required at least {minGranularityRequired} durability points per this interval). The granularity is simply not enough to consider stat effects (such as degradation speed reduction from Cybernetic affinity skill). Please consider decreasing {nameof(this.Lifetime)} or increasing {nameof(this.DurabilityMax)} so there will be more granularity.");
                }
            }

            this.PrepareProtoItemImplant();
        }

        protected virtual void PrepareProtoItemImplant()
        {
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            if (this.durabilityDecreasePerServerUpdate <= 0)
            {
                // non-degradeable
                return;
            }

            if (data.DeltaTime <= 0)
            {
                // not processed
                return;
            }

            // try to degrade durability over time and give experience for cybernetic affinity skill
            var item = data.GameObject;
            var owner = item.Container?.OwnerAsCharacter;
            if (owner is null
                || !owner.ServerIsOnline
                || owner.SharedGetPlayerContainerEquipment() != item.Container)
            {
                // player offline or not an equipped item
                return;
            }

            if (CharacterIdleSystem.ServerIsIdlePlayer(owner))
            {
                return;
            }

            var durabilityDecrease = this.durabilityDecreasePerServerUpdate;
            durabilityDecrease *= owner.SharedGetFinalStatMultiplier(StatName.ImplantDegradationSpeedMultiplier);
            ItemDurabilitySystem.ServerModifyDurability(item, -durabilityDecrease, roundUp: false);

            owner.ServerAddSkillExperience<SkillCyberneticAffinity>(
                data.DeltaTime * SkillCyberneticAffinity.ExperienceAddedPerImplantPerSecond);

            if (!item.IsDestroyed)
            {
                this.ServerUpdateInstalledImplant(data);
            }
        }

        protected virtual void ServerUpdateInstalledImplant(ServerUpdateData data)
        {
        }

        protected override byte[] SharedGetCompatibleContainerSlotsIds()
        {
            // allow placing implants in these slots
            return new byte[]
            {
                (byte)EquipmentType.Implant,
                (byte)EquipmentType.Implant + 1,
                (byte)EquipmentType.Implant + 2
            };
        }
    }

    /// <summary>
    /// Item prototype for implants.
    /// </summary>
    public abstract class ProtoItemEquipmentImplant
        : ProtoItemEquipmentImplant
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}