namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionClientTagControl : BaseUserControl
    {
        public static readonly DependencyProperty ClanTagProperty
            = DependencyProperty.Register("ClanTag",
                                          typeof(string),
                                          typeof(FactionClientTagControl),
                                          new PropertyMetadata(default(string)));

        public string ClanTag
        {
            get => (string)this.GetValue(ClanTagProperty);
            set => this.SetValue(ClanTagProperty, value);
        }
    }
}