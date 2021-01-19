namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDButtonBadgeControl : BaseUserControl
    {
        public static readonly DependencyProperty NumberProperty
            = DependencyProperty.Register(nameof(Number),
                                          typeof(object),
                                          typeof(HUDButtonBadgeControl),
                                          new PropertyMetadata(null));

        public object Number
        {
            get => this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }
    }
}