namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public partial class WindowConstructionMenu : BaseUserControlWithWindow
    {
        private IProtoObjectStructure lastSelectedStructure;

        private Action onClosed;

        private Action<IProtoObjectStructure> onSelected;

        public static WindowConstructionMenu Instance { get; private set; }

        public ViewModelWindowConstructionMenu ViewModel { get; set; }

        public static void Open(
            [NotNull] Action<IProtoObjectStructure> onStructureProtoSelected,
            [NotNull] Action onClosed)
        {
            var isNewControl = false;
            var control = Instance;
            if (control == null)
            {
                control = new WindowConstructionMenu();
                Instance = control;
                isNewControl = true;
            }

            control.onSelected = onStructureProtoSelected;
            control.onClosed = onClosed;

            if (isNewControl)
            {
                Api.Client.UI.LayoutRootChildren.Add(control);
                control.Window.Open();
                return;
            }

            if (control.WindowState == GameWindowState.Closed
                || control.WindowState == GameWindowState.Closing)
            {
                control.Window.Open();
            }
        }

        public void Close()
        {
            this.Window.Close(DialogResult.Cancel);
        }

        protected override void InitControlWithWindow()
        {
        }

        protected override void WindowClosed()
        {
            this.lastSelectedStructure = this.ViewModel.SelectedStructure?.ProtoStructure;
            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
        }

        protected override void WindowClosing()
        {
            this.onClosed?.Invoke();
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();

            if (this.ViewModel != null)
            {
                // re-opening before closed
                return;
            }

            this.ViewModel = new ViewModelWindowConstructionMenu();
            this.ViewModel.StructureSelectedCallback = this.OnSelectedHandler;
            this.DataContext = this.ViewModel;
            // workaround for NoesisGUI bug (it should go before the DataContext is assigned)
            this.ViewModel.SelectStructure(this.lastSelectedStructure);
        }

        private void OnSelectedHandler(IProtoObjectStructure selectedProtoStructure)
        {
            if (selectedProtoStructure == null)
            {
                return;
            }

            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (!selectedProtoStructure.ConfigBuild.CheckStageCanBeBuilt(character))
            {
                Api.Logger.Warning(
                    $"Cannot build selected structure {selectedProtoStructure.ShortId} - not enough resources.");
                return;
            }

            //this.ViewModel.StructureSelectedCallback = null;
            this.CloseWindow();
            this.onSelected.Invoke(selectedProtoStructure);
        }
    }
}