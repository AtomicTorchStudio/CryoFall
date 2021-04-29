namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class VehicleNameEditorControl : BaseUserControl
    {
        public static readonly DependencyProperty VehicleProperty
            = DependencyProperty.Register("Vehicle",
                                          typeof(IDynamicWorldObject),
                                          typeof(VehicleNameEditorControl),
                                          new PropertyMetadata(default(IDynamicWorldObject),
                                                               VehiclePropertyChangedHandler));

        private Grid layoutRoot;

        private ViewModelVehicleNameEditorControl viewModel;

        public IDynamicWorldObject Vehicle
        {
            get => (IDynamicWorldObject)this.GetValue(VehicleProperty);
            set => this.SetValue(VehicleProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.Refresh();
        }

        private static void VehiclePropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VehicleNameEditorControl)d).Refresh();
        }

        private void Refresh()
        {
            if (this.viewModel is not null)
            {
                this.layoutRoot.DataContext = null;
                this.viewModel.Dispose();
                this.viewModel = null;
            }

            if (this.isLoaded
                && this.Vehicle is not null)
            {
                this.layoutRoot.DataContext
                    = this.viewModel
                          = new ViewModelVehicleNameEditorControl(this.Vehicle);
            }
        }
    }
}