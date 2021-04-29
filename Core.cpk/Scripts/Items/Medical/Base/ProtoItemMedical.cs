namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
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
        public override bool CanBeSelectedInVehicle => true;

        public abstract double CooldownDuration { get; }

        public IReadOnlyList<EffectAction> Effects { get; private set; }

        public virtual float FoodRestore => 0;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public virtual float HealthRestore => 0;

        public string ItemUseCaption => ItemUseCaptions.Use;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public abstract double MedicalToxicity { get; }

        public virtual float StaminaRestore => 0;

        public virtual float WaterRestore => 0;

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var item = data.Item;
            var character = Client.Characters.CurrentPlayerCharacter;
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;

            if (!this.SharedCanUse(character, stats))
            {
                return false;
            }

            if (this.SharedHasActiveCooldown(item, character))
            {
                return false;
            }

            this.CallServer(_ => _.ServerRemote_Use(item));
            return true;
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            if (this.CooldownDuration > 0)
            {
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                var statusEffectMedicalCooldown = Api.GetProtoEntity<StatusEffectMedicalCooldown>();
                var icon = new Rectangle()
                {
                    Fill = Api.Client.UI.GetTextureBrush(
                        ((IProtoStatusEffect)statusEffectMedicalCooldown).Icon),
                    Width = 26,
                    Height = 26,
                    UseLayoutRounding = true
                };

                var controlInfoEntry = ItemTooltipInfoEntryControl.Create(
                    statusEffectMedicalCooldown.Name,
                    ClientTimeFormatHelper.FormatTimeDuration(this.CooldownDuration));
                controlInfoEntry.Margin = new Thickness(4, 0, 0, -5);
                controlInfoEntry.VerticalAlignment = VerticalAlignment.Center;
                controlInfoEntry.Foreground
                    = controlInfoEntry.ValueBrush
                          = Api.Client.UI.GetApplicationResource<Brush>("BrushColorAltLabelForeground");

                stackPanel.Children.Add(icon);
                stackPanel.Children.Add(controlInfoEntry);
                stackPanel.Margin = new Thickness(0, 7, 0, 7);
                controls.Add(stackPanel);
            }

            if (this.Effects.Count > 0)
            {
                controls.Add(ItemTooltipInfoEffectActionsControl.Create(this.Effects));
            }
        }

        protected override string GenerateIconPath()
        {
            return "Items/Medical/" + this.GetType().Name;
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

            if (this.CooldownDuration > 0)
            {
                // medical cooldown is added automatically and it's hidden
                // (the cooldown duration is displayed separately in the tooltip)
                effects.WillAddEffect<StatusEffectMedicalCooldown>(
                    intensity: Math.Min(this.CooldownDuration / StatusEffectMedicalCooldown.MaxDuration, 1),
                    isHidden: true);
            }

            this.PrepareEffects(effects);
            this.Effects = effects.ToReadOnly();
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

        [RemoteCallSettings(DeliveryMode.ReliableOrdered,
                            timeInterval: 0.2,
                            clientMaxSendQueueSize: 20)]
        private void ServerRemote_Use(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            if (!this.SharedCanUse(character, stats))
            {
                return;
            }

            if (this.SharedHasActiveCooldown(item, character))
            {
                return;
            }

            this.ServerOnUse(character, stats);

            Logger.Important(character + " has used " + item);

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));
        }

        private bool SharedHasActiveCooldown(IItem item, ICharacter character)
        {
            if (this.CooldownDuration <= 0)
            {
                // this item is not subject to the medical cooldown
                return false;
            }

            if (!character.SharedHasPerk(StatName.PerkCannotUseMedicalItems))
            {
                return false;
            }

            Logger.Info("Cannot use a medicine - under a medical cooldown: " + item, character);

            if (IsClient)
            {
                StatusEffectMedicalCooldown.ClientShowCooldownNotification();
            }

            return true;
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