namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    /// <summary>
    /// Item prototype for medical items (with state).
    /// </summary>
    public abstract class ProtoItemMedical
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWithFreshness
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemMedical
        where TPrivateState : BasePrivateState, IItemWithFreshnessPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemMedical()
        {
            this.Icon = new TextureResource("Items/Medical/" + this.GetType().Name);
        }

        public override bool CanBeSelectedInVehicle => true;

        public IReadOnlyList<EffectAction> Effects { get; private set; }

        public virtual float FoodRestore => 0;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public virtual float HealthRestore => 0;

        public override ITextureResource Icon { get; }

        public string ItemUseCaption => ItemUseCaptions.Use;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public abstract double MedicalToxicity { get; }

        public virtual float StaminaRestore => 0;

        public virtual float WaterRestore => 0;

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            if (this.SharedCanUse(character, stats))
            {
                this.CallServer(_ => _.ServerRemote_Use(data.Item));
                return true;
            }

            return false;
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            if (this.Effects.Count > 0)
            {
                controls.Add(ItemTooltipInfoEffectActionsControl.Create(this.Effects));
            }
        }

        protected virtual void PrepareEffects(EffectActionsList effects)
        {
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.AltClickToUseItem);
        }

        protected virtual void PrepareProtoItemMedical()
        {
        }

        protected sealed override void PrepareProtoItemWithFreshness()
        {
            base.PrepareProtoItemWithFreshness();
            this.PrepareProtoItemMedical();

            var effects = new EffectActionsList();
            this.PrepareEffects(effects);
            this.Effects = effects.ToReadOnly();
            ;
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedical;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        protected virtual void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            if (this.MedicalToxicity > 0)
            {
                character.ServerAddStatusEffect<StatusEffectMedicineOveruse>(intensity: this.MedicalToxicity);
            }

            if (this.StaminaRestore != 0)
            {
                currentStats.SharedSetStaminaCurrent(currentStats.StaminaCurrent + this.StaminaRestore);
            }

            if (this.HealthRestore != 0)
            {
                if (this.HealthRestore > 0)
                {
                    currentStats.ServerSetHealthCurrent(currentStats.HealthCurrent + this.HealthRestore);
                }
                else
                {
                    currentStats.ServerReduceHealth(-this.HealthRestore, this);
                }
            }

            if (this.FoodRestore != 0)
            {
                currentStats.ServerSetFoodCurrent(currentStats.FoodCurrent + this.FoodRestore);
            }

            if (this.WaterRestore != 0)
            {
                currentStats.ServerSetWaterCurrent(currentStats.WaterCurrent + this.WaterRestore);
            }

            foreach (var effect in this.Effects)
            {
                effect.Execute(new EffectActionContext(character));
            }
        }

        protected virtual bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            return true;
        }

        private void ServerRemote_Use(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            if (!this.SharedCanUse(character, stats))
            {
                return;
            }

            this.ServerOnUse(character, stats);

            Logger.Important(character + " has used " + item);

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));
        }
    }

    /// <summary>
    /// Item prototype for medical items (without state).
    /// </summary>
    public abstract class ProtoItemMedical
        : ProtoItemMedical
            <ItemWithFreshnessPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}