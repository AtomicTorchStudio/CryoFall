namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets.Data
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Turrets;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowTurret : BaseViewModel
    {
        private readonly IClientItemsContainer containerAmmo;

        private readonly ObjectTurretPrivateState privateState;

        private readonly IStaticWorldObject worldObjectTurret;

        private TurretMode selectedTurretMode;

        public ViewModelWindowTurret(IStaticWorldObject worldObjectTurret)
        {
            this.worldObjectTurret = worldObjectTurret;
            this.privateState = worldObjectTurret.GetPrivateState<ObjectTurretPrivateState>();
            this.TurretModes = Enum.GetValues(typeof(TurretMode))
                                   .Cast<TurretMode>()
                                   .ExceptOne(TurretMode.Disabled)
                                   .Select(e => new ViewModelEnum<TurretMode>(e))
                                   .OrderBy(vm => vm.Order)
                                   .ToArray();

            this.selectedTurretMode = this.privateState.TurretMode;
            this.privateState.ClientSubscribe(_ => _.TurretMode,
                                              _ => this.RefreshTurretMode(),
                                              this);

            this.containerAmmo = this.privateState.ContainerAmmo as IClientItemsContainer;
            if (this.containerAmmo is not null)
            {
                this.containerAmmo.StateHashChanged += this.ContainerAmmoStateHashChanged;

                this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(this.containerAmmo)
                {
                    ContainerTitle = CoreStrings.Vehicle_Ammo,
                    IsManagementButtonsVisible = false
                };

                this.ProtoItemWeapon =
                    ((BaseItemsContainerTurretAmmo)this.ViewModelItemsContainerExchange.Container.ProtoGameObject)
                    .ProtoWeapon;
            }

            this.RefreshTurretMode();
        }

        public Brush AmmoIcon
            => this.ProtoItemAmmo is null
                   ? null
                   : Client.UI.GetTextureBrush(this.ProtoItemAmmo.Icon);

        public IProtoItemAmmo ProtoItemAmmo
            => this.containerAmmo?.Items.LastOrDefault()?.ProtoGameObject
                   as IProtoItemAmmo;

        public IProtoItemWeapon ProtoItemWeapon { get; }

        public ViewModelEnum<TurretMode> SelectedTurretMode
        {
            get => new(this.selectedTurretMode);
            set => this.SetSelectedTurretMode(value.Value, sendToServer: true);
        }

        public ViewModelEnum<TurretMode>[] TurretModes { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        protected override void DisposeViewModel()
        {
            if (this.containerAmmo is not null)
            {
                this.containerAmmo.StateHashChanged -= this.ContainerAmmoStateHashChanged;
            }

            base.DisposeViewModel();
        }

        private void ContainerAmmoStateHashChanged()
        {
            this.NotifyPropertyChanged(nameof(this.ProtoItemAmmo));
            this.NotifyPropertyChanged(nameof(this.AmmoIcon));
        }

        private void RefreshTurretMode()
        {
            this.SetSelectedTurretMode(this.privateState.TurretMode,
                                       sendToServer: false);
        }

        private void SetSelectedTurretMode(TurretMode mode, bool sendToServer)
        {
            if (this.selectedTurretMode == mode)
            {
                return;
            }

            this.selectedTurretMode = mode;
            this.NotifyPropertyChanged(nameof(this.SelectedTurretMode));

            if (sendToServer)
            {
                var protoObjectTurret = ((IProtoObjectTurret)(this.worldObjectTurret.ProtoGameObject));
                protoObjectTurret.ClientSetTurretMode(this.worldObjectTurret, mode);
            }
        }
    }
}