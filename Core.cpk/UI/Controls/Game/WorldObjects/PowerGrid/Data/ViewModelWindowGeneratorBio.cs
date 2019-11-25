namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowGeneratorBio : BaseViewModel
    {
        private readonly IStaticWorldObject objectGenerator;

        private readonly ObjectGeneratorBioPrivateState privateState;

        private readonly ProtoObjectGeneratorBio protoGenerator;

        public ViewModelWindowGeneratorBio(
            IStaticWorldObject objectGenerator,
            ObjectGeneratorBioPrivateState privateState)
        {
            this.objectGenerator = objectGenerator;
            this.protoGenerator = (ProtoObjectGeneratorBio)objectGenerator.ProtoGameObject;
            this.privateState = privateState;

            this.privateState.ClientSubscribe(
                _ => _.OrganicAmount,
                _ => this.NotifyPropertyChanged(nameof(this.OrganicAmount)),
                this);

            var character = ClientCurrentCharacterHelper.Character;
            ClientContainersExchangeManager.Register(
                this,
                this.InputItemsContainer,
                allowedTargets: new[]
                {
                    character.SharedGetPlayerContainerInventory(),
                    character.SharedGetPlayerContainerHotbar()
                });

            this.Refresh();
        }

        public string ElectricityProductionInfoText
        {
            get
            {
                this.protoGenerator.SharedGetElectricityProduction(this.objectGenerator,
                                                                   out var currentProduction,
                                                                   out var maxProduction);

                return string.Format(
                    ViewModelWindowGeneratorWithFuel.ElectricityProductionInfoTextFormat,
                    currentProduction.ToString("F1"),
                    maxProduction.ToString("F1"));
            }
        }

        public IItemsContainer InputItemsContainer => this.privateState.InputItemsCointainer;

        public ushort OrganicAmount => (ushort)Math.Round(this.privateState.OrganicAmount,
                                                          MidpointRounding.AwayFromZero);

        public ushort OrganicCapacity => this.protoGenerator.OrganicCapacity;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientContainersExchangeManager.Unregister(this);
        }

        private void Refresh()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.ElectricityProductionInfoText));

            ClientTimersSystem.AddAction(
                delaySeconds: 0.5,
                this.Refresh);
        }
    }
}