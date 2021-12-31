namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SkinsListControl : BaseUserControl
    {
        public static readonly DependencyProperty IsOwnedLabelDisplayedProperty
            = DependencyProperty.Register("IsOwnedLabelDisplayed",
                                          typeof(bool),
                                          typeof(SkinsListControl),
                                          new PropertyMetadata(true));

        public static readonly DependencyProperty IsSkinNameDisplayedProperty
            = DependencyProperty.Register(nameof(IsSkinNameDisplayed),
                                          typeof(bool),
                                          typeof(SkinsListControl),
                                          new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty SkinsProperty
            = DependencyProperty.Register("Skins",
                                          typeof(IReadOnlyList<ViewModelSkin>),
                                          typeof(SkinsListControl),
                                          new PropertyMetadata(null));

        public bool IsOwnedLabelDisplayed
        {
            get => (bool)this.GetValue(IsOwnedLabelDisplayedProperty);
            set => this.SetValue(IsOwnedLabelDisplayedProperty, value);
        }

        public bool IsSkinNameDisplayed
        {
            get => (bool)this.GetValue(IsSkinNameDisplayedProperty);
            set => this.SetValue(IsSkinNameDisplayedProperty, value);
        }

        public IReadOnlyList<ViewModelSkin> Skins
        {
            get => (IReadOnlyList<ViewModelSkin>)this.GetValue(SkinsProperty);
            set => this.SetValue(SkinsProperty, value);
        }
    }
}