namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelVehicleSchematic : BaseViewModel
    {
        public ViewModelVehicleSchematic(IProtoVehicle protoVehicle)
        {
            this.ProtoVehicle = protoVehicle;
            if (protoVehicle is null)
            {
                return;
            }

            this.UpdateIsCanBuild();
            this.SubscribeToContainersEvents();
        }

        public uint BuildRequiredElectricityAmount => this.ProtoVehicle.BuildRequiredElectricityAmount;

        public IReadOnlyList<ProtoItemWithCount> BuildRequiredItems => this.ProtoVehicle.BuildRequiredItems;

        public string Description => this.ProtoVehicle.Description;

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoVehicle.Icon);

        public bool IsCanBuild { get; private set; }

        public bool IsSelected { get; set; }

        public IProtoVehicle ProtoVehicle { get; }

        public string Title => this.ProtoVehicle.Name;

        public override string ToString()
        {
            return this.ProtoVehicle?.ToString() ?? string.Empty;
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            if (this.ProtoVehicle is not null)
            {
                this.UnsubscribeFromContainersEvents();
            }
        }

        private void ContainersItemsResetHandler()
        {
            this.UpdateIsCanBuild();
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            this.UpdateIsCanBuild();
        }

        private void SubscribeToContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged +=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void UnsubscribeFromContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void UpdateIsCanBuild()
        {
            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var checkResult = this.ProtoVehicle.SharedPlayerCanBuild(currentPlayerCharacter);
            this.IsCanBuild = checkResult == VehicleCanBuildCheckResult.Success
                              || checkResult == VehicleCanBuildCheckResult.NeedsFreeSpace;
        }
    }
}