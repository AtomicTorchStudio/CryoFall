namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates.Data;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public partial class WindowCrateIconSelector : BaseUserControlWithWindow
    {
        private readonly IReadOnlyCollection<IItem> existingItems;

        private readonly IProtoEntity selectedProtoEntity;

        private ViewModelWindowCrateIconSelector viewModel;

        public WindowCrateIconSelector(IProtoEntity selectedProtoEntity, IReadOnlyCollection<IItem> existingItems)
        {
            this.selectedProtoEntity = selectedProtoEntity;
            this.existingItems = existingItems;
        }

        public WindowCrateIconSelector()
        {
        }

        public ViewModelWindowCrateIconSelector ViewModel => this.viewModel;

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = this.viewModel =
                                   new ViewModelWindowCrateIconSelector(
                                       this.selectedProtoEntity,
                                       this.existingItems,
                                       callbackSave: () => this.CloseWindow(DialogResult.OK),
                                       callbackCancel: () => this.CloseWindow(DialogResult.Cancel));
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}