namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class TechGroupTooltipControl : BaseUserControl
    {
        public static readonly DependencyProperty RequirementsOnlyProperty
            = DependencyProperty.Register(nameof(RequirementsOnly),
                                          typeof(bool),
                                          typeof(TechGroupTooltipControl),
                                          new PropertyMetadata(default(bool)));

        public bool RequirementsOnly
        {
            get => (bool)this.GetValue(RequirementsOnlyProperty);
            set => this.SetValue(RequirementsOnlyProperty, value);
        }

        protected override void InitControl()
        {
        }
    }
}