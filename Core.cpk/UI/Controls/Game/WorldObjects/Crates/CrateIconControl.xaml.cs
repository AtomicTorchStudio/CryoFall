namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CrateIconControl : BaseUserControl
    {
        public static readonly DependencyProperty WorldObjectCrateProperty =
            DependencyProperty.Register("WorldObjectCrate",
                                        typeof(IStaticWorldObject),
                                        typeof(CrateIconControl),
                                        new PropertyMetadata(default(IStaticWorldObject)));

        private ViewModelCrateIconControl viewModel;

        public IStaticWorldObject WorldObjectCrate
        {
            get => this.GetValue(WorldObjectCrateProperty) as IStaticWorldObject;
            set => this.SetValue(WorldObjectCrateProperty, value);
        }

        protected override void OnLoaded()
        {
            if (this.WorldObjectCrate is null)
            {
                return;
            }

            var protoObjectCrate = (IProtoObjectCrate)this.WorldObjectCrate.ProtoGameObject;
            if (!protoObjectCrate.IsSupportItemIcon)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.DataContext = this.viewModel = new ViewModelCrateIconControl(this.WorldObjectCrate);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}