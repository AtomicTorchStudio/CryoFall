namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelItemTooltipCurrentAmmoControl : BaseViewModel
    {
        private IItem item;

        private IProtoItemAmmo protoItemAmmo;

        public ViewModelItemTooltipCurrentAmmoControl(IItem item)
        {
            this.Item = item;
        }

        public ushort AmmoCountCurrent { get; set; } = 10;

        public ushort AmmoCountMax { get; set; } = 20;

        public Brush AmmoIcon
            => this.ProtoItemAmmo is null
                   ? null
                   : Client.UI.GetTextureBrush(this.ProtoItemAmmo.Icon);

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

                this.ProtoItemAmmo = this.Item?.GetPrivateState<WeaponPrivateState>()
                                         .CurrentProtoItemAmmo;

                weaponPrivateState.ClientSubscribe(_ => _.AmmoCount,            this.AmmoCountChanged,            this);
                weaponPrivateState.ClientSubscribe(_ => _.CurrentProtoItemAmmo, this.CurrentProtoItemAmmoChanged, this);
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
                this.NotifyPropertyChanged(nameof(this.AmmoIcon));
                this.NotifyThisPropertyChanged();
            }
        }

        private void AmmoCountChanged(ushort ammoCount)
        {
            this.AmmoCountCurrent = ammoCount;
        }

        private void CurrentProtoItemAmmoChanged(IProtoItemAmmo currentProtoItemAmmo)
        {
            this.ProtoItemAmmo = this.Item?.GetPrivateState<WeaponPrivateState>()
                                     .CurrentProtoItemAmmo;
        }
    }
}