namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.StatModificationDisplay
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.StatModificationDisplay.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class StatModificationDisplay : BaseUserControl
    {
        public static readonly DependencyProperty HideDefenseStatsProperty
            = DependencyProperty.Register(nameof(HideDefenseStats),
                                          typeof(bool),
                                          typeof(StatModificationDisplay),
                                          new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty StatsDictionaryProperty
            = DependencyProperty.Register(nameof(StatsDictionary),
                                          typeof(IReadOnlyStatsDictionary),
                                          typeof(StatModificationDisplay),
                                          new PropertyMetadata(default(IReadOnlyStatsDictionary)));

        private FrameworkElement layoutRoot;

        private ViewModelStatModificationDisplay viewModel;

        public StatModificationDisplay()
        {
        }

        public StatModificationDisplay(IReadOnlyStatsDictionary statsDictionary, bool hideDefenseStats)
        {
            this.StatsDictionary = statsDictionary;
            this.HideDefenseStats = hideDefenseStats;
        }

        public bool HideDefenseStats
        {
            get => (bool)this.GetValue(HideDefenseStatsProperty);
            set => this.SetValue(HideDefenseStatsProperty, value);
        }

        public IReadOnlyStatsDictionary StatsDictionary
        {
            get => (IReadOnlyStatsDictionary)this.GetValue(StatsDictionaryProperty);
            set => this.SetValue(StatsDictionaryProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext
                = this.viewModel
                      = new ViewModelStatModificationDisplay(this.StatsDictionary,
                                                             this.HideDefenseStats);
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}