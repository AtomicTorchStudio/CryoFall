namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelHotbarItemWeaponOverlayControl : BaseViewModel
    {
        private readonly Action ammoChangedCallback;

        private readonly WeaponState weaponState;

        private IItem item;

        private IProtoItemAmmo protoItemAmmo;

        public ViewModelHotbarItemWeaponOverlayControl(Action ammoChangedCallback)
        {
            this.ammoChangedCallback = ammoChangedCallback;
            this.weaponState = ClientCurrentCharacterHelper.PrivateState.WeaponState;

            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged += this.ItemAddedOrRemovedOrCountChangedHandler;

            ClientCurrentCharacterVehicleContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterVehicleContainersHelper.ItemAddedOrRemovedOrCountChanged += this.ItemAddedOrRemovedOrCountChangedHandler;

            if (this.weaponState is null)
            {
                return;
            }

            this.weaponState.ClientWeaponReloadingStateChanged += this.WeaponReloadingStateChangedHandler;
            this.weaponState.ClientActiveWeaponChanged += this.ActiveWeaponChangedHandler;
        }

        public ushort AmmoCountCurrent { get; set; } = 10;

        public ushort AmmoCountMax { get; set; } = 20;

        public Brush AmmoIcon => Client.UI.GetTextureBrush(this.protoItemAmmo?.Icon);

        public int AmmoTotalAvailable { get; private set; }

        public bool IsReloading { get; private set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item is not null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;

                if (this.item is null)
                {
                    return;
                }

                var weaponPrivateState = this.item.GetPrivateState<WeaponPrivateState>();
                var protoItemWeapon = (IProtoItemWeapon)this.item.ProtoGameObject;

                this.AmmoCountMax = protoItemWeapon.AmmoCapacity;
                this.AmmoCountCurrent = weaponPrivateState.AmmoCount;

                this.UpdateCurrentProtoItem();

                weaponPrivateState.ClientSubscribe(_ => _.AmmoCount,            this.AmmoCountChanged,            this);
                weaponPrivateState.ClientSubscribe(_ => _.CurrentProtoItemAmmo, this.CurrentProtoItemAmmoChanged, this);

                this.VisibilityAmmoOverlay = protoItemWeapon.CompatibleAmmoProtos.Count > 0
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;

                this.UpdateWeaponReloadingState();
            }
        }

        public IProtoItemAmmo ProtoItemAmmo
        {
            get => this.protoItemAmmo;
            private set
            {
                if (this.protoItemAmmo == value)
                {
                    return;
                }

                this.protoItemAmmo = value;

                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.AmmoIcon));
                this.ammoChangedCallback?.Invoke();

                this.RefreshAmmoTotalAvailable();
            }
        }

        //public double ReloadDurationSeconds { get; private set; }

        public Visibility VisibilityAmmoOverlay { get; private set; }

        //public double WeaponSwitchDurationSeconds { get; private set; }

        public double WeaponReadyCountdownDuration { get; private set; }

        protected override void DisposeViewModel()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -= this.ItemAddedOrRemovedOrCountChangedHandler;

            ClientCurrentCharacterVehicleContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterVehicleContainersHelper.ItemAddedOrRemovedOrCountChanged -= this.ItemAddedOrRemovedOrCountChangedHandler;

            if (this.weaponState is not null)
            {
                this.weaponState.ClientWeaponReloadingStateChanged -= this.WeaponReloadingStateChangedHandler;
                this.weaponState.ClientActiveWeaponChanged -= this.ActiveWeaponChangedHandler;
            }

            base.DisposeViewModel();
        }

        private void ActiveWeaponChangedHandler()
        {
            this.RefreshReloadOrWeaponSwitchDurationSeconds();
        }

        private void AmmoCountChanged(ushort ammoCount)
        {
            this.AmmoCountCurrent = ammoCount;
        }

        private void ContainersItemsResetHandler()
        {
            this.RefreshAmmoTotalAvailable();
        }

        private void CurrentProtoItemAmmoChanged(IProtoItemAmmo currentProtoItemAmmo)
        {
            this.UpdateCurrentProtoItem();
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            if (ReferenceEquals(item.ProtoItem, this.protoItemAmmo))
            {
                this.RefreshAmmoTotalAvailable();
            }
        }

        private void RefreshAmmoTotalAvailable()
        {
            if (this.protoItemAmmo is null)
            {
                this.AmmoTotalAvailable = 0;
            }
            else
            {
                this.AmmoTotalAvailable = WeaponAmmoSystem.SharedGetTotalAvailableAmmo(
                    this.protoItemAmmo,
                    Client.Characters.CurrentPlayerCharacter);
            }
        }

        private void RefreshReloadOrWeaponSwitchDurationSeconds()
        {
            if (this.item != this.weaponState.ItemWeapon)
            {
                this.IsReloading = false;
                // stop any countdown animation
                this.WeaponReadyCountdownDuration = 0;
                return;
            }

            var reloadingState = this.weaponState.WeaponReloadingState;
            var weaponSwitchDurationSeconds = this.weaponState.CooldownSecondsRemains;

            var reloadDurationSeconds = reloadingState is null
                                        || !ReferenceEquals(reloadingState.Item, this.item)
                                            ? 0
                                            : reloadingState.SecondsToReloadRemains;

            this.IsReloading = reloadDurationSeconds > 0;

            // assign twice in order to reset the animated countdown
            this.WeaponReadyCountdownDuration = 0;
            this.WeaponReadyCountdownDuration = Math.Max(weaponSwitchDurationSeconds,
                                                         reloadDurationSeconds);
        }

        private void UpdateCurrentProtoItem()
        {
            var reloadingState = this.weaponState.WeaponReloadingState;
            if (reloadingState is not null
                && reloadingState.Item == this.item)
            {
                this.ProtoItemAmmo = reloadingState.ProtoItemAmmo;
            }
            else
            {
                this.ProtoItemAmmo = this.Item?.GetPrivateState<WeaponPrivateState>()
                                         .CurrentProtoItemAmmo;
            }
        }

        private void UpdateWeaponReloadingState()
        {
            this.RefreshReloadOrWeaponSwitchDurationSeconds();
            this.UpdateCurrentProtoItem();
        }

        private void WeaponReloadingStateChangedHandler()
        {
            this.UpdateWeaponReloadingState();
        }
    }
}