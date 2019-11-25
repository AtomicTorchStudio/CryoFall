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
    using AtomicTorch.CBND.GameApi.Resources;

    public class ViewModelHotbarItemWeaponOverlayControl : BaseViewModel
    {
        private readonly Action ammoChangedCallback;

        private readonly WeaponState weaponState;

        private ITextureResource ammoIconResource;

        private IItem item;

        private IProtoItemAmmo protoItemAmmo;

        public ViewModelHotbarItemWeaponOverlayControl(Action ammoChangedCallback)
        {
            this.ammoChangedCallback = ammoChangedCallback;
            this.weaponState = ClientCurrentCharacterHelper.PrivateState.WeaponState;

            if (this.weaponState == null)
            {
                this.ReloadDurationSeconds = 0;
                return;
            }

            this.weaponState.ClientWeaponReloadingStateChanged += this.WeaponReloadingStateChangedHandler;
            this.weaponState.ClientActiveWeaponChanged += this.ActiveWeaponChangedHandler;

            this.UpdateWeaponReloadingState();
        }

        public ushort AmmoCountCurrent { get; set; } = 10;

        public ushort AmmoCountMax { get; set; } = 20;

        public Brush AmmoIcon { get; set; }

        public ITextureResource AmmoIconResource
        {
            get => this.ammoIconResource;
            set
            {
                if (this.ammoIconResource == value)
                {
                    return;
                }

                this.ammoIconResource = value;
                if (value == null)
                {
                    this.AmmoIcon = null;
                    return;
                }

                this.AmmoIcon = Client.UI.GetTextureBrush(this.ammoIconResource);
            }
        }

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

                if (this.item != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;

                if (this.item == null)
                {
                    return;
                }

                var weaponPrivateState = this.item.GetPrivateState<WeaponPrivateState>();
                var protoItemWeapon = ((IProtoItemWeapon)this.item.ProtoGameObject);

                this.AmmoCountMax = protoItemWeapon.AmmoCapacity;
                this.AmmoCountCurrent = weaponPrivateState.AmmoCount;

                this.UpdateCurrentProtoItem();

                weaponPrivateState.ClientSubscribe(_ => _.AmmoCount,            this.AmmoCountChanged,            this);
                weaponPrivateState.ClientSubscribe(_ => _.CurrentProtoItemAmmo, this.CurrentProtoItemAmmoChanged, this);

                this.VisibilityAmmoOverlay = protoItemWeapon.CompatibleAmmoProtos.Count > 0
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;
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
                this.AmmoIconResource = value?.Icon;

                this.NotifyThisPropertyChanged();
                this.ammoChangedCallback?.Invoke();
            }
        }

        public double ReloadDurationSeconds { get; private set; }

        public Visibility VisibilityAmmoOverlay { get; private set; }

        public double WeaponSwitchDurationSeconds { get; private set; }

        protected override void DisposeViewModel()
        {
            if (this.weaponState != null)
            {
                this.weaponState.ClientWeaponReloadingStateChanged -= this.WeaponReloadingStateChangedHandler;
                this.weaponState.ClientActiveWeaponChanged -= this.ActiveWeaponChangedHandler;
            }

            base.DisposeViewModel();
        }

        private void ActiveWeaponChangedHandler()
        {
            if (this.item != this.weaponState.ItemWeapon)
            {
                // stop any countdown animation
                this.WeaponSwitchDurationSeconds = 0;
                return;
            }

            // assign twice in order to reset the animated countdown
            this.WeaponSwitchDurationSeconds = 0;
            this.WeaponSwitchDurationSeconds = this.weaponState.CooldownSecondsRemains;
        }

        private void AmmoCountChanged(ushort ammoCount)
        {
            this.AmmoCountCurrent = ammoCount;
        }

        private void CurrentProtoItemAmmoChanged(IProtoItemAmmo currentProtoItemAmmo)
        {
            this.UpdateCurrentProtoItem();
        }

        private void UpdateCurrentProtoItem()
        {
            var reloadingState = this.weaponState.WeaponReloadingState;
            if (reloadingState != null
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
            var reloadingState = this.weaponState.WeaponReloadingState;
            if (reloadingState == null
                || reloadingState.Item != this.item)
            {
                this.ReloadDurationSeconds = 0;
            }
            else
            {
                // assign twice in order to reset the animated countdown
                this.ReloadDurationSeconds = 0;
                this.ReloadDurationSeconds = reloadingState.SecondsToReloadRemains;
            }

            this.IsReloading = reloadingState != null
                               && reloadingState.Item == this.item;

            this.UpdateCurrentProtoItem();
        }

        private void WeaponReloadingStateChangedHandler()
        {
            this.UpdateWeaponReloadingState();
        }
    }
}