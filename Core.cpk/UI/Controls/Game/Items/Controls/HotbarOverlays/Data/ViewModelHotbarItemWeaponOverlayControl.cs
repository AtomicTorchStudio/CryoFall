namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
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
        private readonly WeaponState weaponState;

        private ITextureResource ammoIconResource;

        private IItem item;

        public ViewModelHotbarItemWeaponOverlayControl()
        {
            if (IsDesignTime)
            {
                this.AmmoIcon = Brushes.Red;
                return;
            }

            var characterState = ClientCurrentCharacterHelper.PrivateState;
            this.weaponState = characterState.WeaponState;

            if (this.weaponState == null)
            {
                this.ReloadDurationSeconds = 0;
                return;
            }

            this.weaponState.WeaponReloadingStateChanged += this.WeaponReloadingStateChanged;
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

                this.AmmoCountMax = ((IProtoItemWeapon)this.item.ProtoGameObject).AmmoCapacity;
                this.AmmoCountCurrent = weaponPrivateState.AmmoCount;

                this.UpdateAmmoIconResource();

                weaponPrivateState.ClientSubscribe(_ => _.AmmoCount,            this.AmmoCountChanged,            this);
                weaponPrivateState.ClientSubscribe(_ => _.CurrentProtoItemAmmo, this.CurrentProtoItemAmmoChanged, this);
            }
        }

        public double ReloadDurationSeconds { get; private set; }

        protected override void DisposeViewModel()
        {
            if (this.weaponState != null)
            {
                this.weaponState.WeaponReloadingStateChanged -= this.WeaponReloadingStateChanged;
            }

            base.DisposeViewModel();
        }

        private void AmmoCountChanged(ushort ammoCount)
        {
            this.AmmoCountCurrent = ammoCount;
        }

        private void CurrentProtoItemAmmoChanged(IProtoItemAmmo currentProtoItemAmmo)
        {
            this.UpdateAmmoIconResource();
        }

        private void UpdateAmmoIconResource()
        {
            var reloadingState = this.weaponState.WeaponReloadingState;
            if (reloadingState != null
                && reloadingState.Item == this.item)
            {
                this.AmmoIconResource = reloadingState.ProtoItemAmmo?.Icon;
                return;
            }

            this.AmmoIconResource = this.Item?.GetPrivateState<WeaponPrivateState>()
                                        .CurrentProtoItemAmmo?.Icon;
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

            this.UpdateAmmoIconResource();
        }

        private void WeaponReloadingStateChanged()
        {
            this.UpdateWeaponReloadingState();
        }
    }
}