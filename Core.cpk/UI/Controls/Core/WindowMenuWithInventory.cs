namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class WindowMenuWithInventory : BaseContentControl
    {
        public static readonly DependencyProperty SoundClosingProperty =
            DependencyProperty.Register(
                nameof(SoundClosing),
                typeof(SoundUI),
                typeof(WindowMenuWithInventory),
                new PropertyMetadata(default(SoundUI)));

        public static readonly DependencyProperty SoundOpeningProperty =
            DependencyProperty.Register(
                nameof(SoundOpening),
                typeof(SoundUI),
                typeof(WindowMenuWithInventory),
                new PropertyMetadata(default(SoundUI)));

        private ItemsContainerControl containerInventoryControl;

        private ViewModelWindowMenuWithInventory viewModel;

        static WindowMenuWithInventory()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(WindowMenuWithInventory),
                new FrameworkPropertyMetadata(typeof(WindowMenuWithInventory)));
        }

        /// <summary>
        /// <see cref="SoundOpening" /> property comment.
        /// </summary>
        public SoundUI SoundClosing
        {
            get => (SoundUI)this.GetValue(SoundClosingProperty);
            set => this.SetValue(SoundClosingProperty, value);
        }

        /// <summary>
        /// This property is bound via template binding to according window sound (so window play it automatically).
        /// </summary>
        public SoundUI SoundOpening
        {
            get => (SoundUI)this.GetValue(SoundOpeningProperty);
            set => this.SetValue(SoundOpeningProperty, value);
        }

        public GameWindow Window { get; private set; }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.Window = templateRoot.GetByName<GameWindow>("GameWindow");
            this.containerInventoryControl = templateRoot.GetByName<ItemsContainerControl>("ContainerInventoryControl");

            // TODO: make a better workaround for this hack?
            this.Window.LinkedParent =
                LogicalTreeHelper.GetParent((FrameworkElement)VisualTreeHelper.GetParent(templateRoot));
        }

        protected override void OnLoaded()
        {
            this.containerInventoryControl.DataContext = this.viewModel = new ViewModelWindowMenuWithInventory();
        }

        protected override void OnUnloaded()
        {
            this.containerInventoryControl.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}