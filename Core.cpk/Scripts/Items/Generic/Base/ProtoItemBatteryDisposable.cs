namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoItemBatteryDisposable
        : ProtoItemGeneric, IProtoItemFuelElectricity, IProtoItemUsableFromContainer
    {
        public const string NotificationNoPowerBanksEquipped = "You don't have any powerbanks equipped";

        public const string NotificationNoRechargingRequired = "No recharging required";

        public abstract double FuelAmount { get; }

        public string ItemUseCaption => ItemUseCaptions.Use;

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            if (!this.SharedIsRechargeNeeded(ClientCurrentCharacterHelper.Character))
            {
                return false;
            }

            this.CallServer(_ => _.ServerRemote_ConsumeItem(data.Item));
            return true;
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use, "Items/BatteryDisposable/Use");
        }

        private void ServerRemote_ConsumeItem(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            if (!this.SharedIsRechargeNeeded(character))
            {
                return;
            }

            Server.Items.SetCount(item, item.Count - 1);
            CharacterEnergySystem.ServerAddEnergyCharge(character, this.FuelAmount);
        }

        private bool SharedIsRechargeNeeded(ICharacter character)
        {
            var availableCapacity = CharacterEnergySystem.SharedCalculateTotalEnergyCapacity(character);
            var currentCharge = CharacterEnergySystem.SharedCalculateTotalEnergyCharge(character);
            if (currentCharge < availableCapacity)
            {
                // need to recharge
                return true;
            }

            // no need to recharge
            if (IsClient)
            {
                if (availableCapacity > 0)
                {
                    NotificationSystem.ClientShowNotification(NotificationNoRechargingRequired,
                                                              icon: this.Icon);
                }
                else
                {
                    NotificationSystem.ClientShowNotification(NotificationNoPowerBanksEquipped,
                                                              icon: this.Icon);
                }
            }

            return false;
        }
    }
}