namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkResourceOil : BaseUserControl
    {
        public static readonly DependencyProperty IsInfiniteSourceProperty =
            DependencyProperty.Register("IsInfiniteSource",
                                        typeof(bool),
                                        typeof(WorldMapMarkResourceOil),
                                        new PropertyMetadata(default(bool)));

        public bool IsInfiniteSource
        {
            get => (bool)this.GetValue(IsInfiniteSourceProperty);
            set => this.SetValue(IsInfiniteSourceProperty, value);
        }

        protected override void OnLoaded()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.GetByName<ContentControl>("TextBlockTooltip").Content
                = this.IsInfiniteSource
                      ? CoreStrings.WorldMapMarkResourceOil_Infinite_Tooltip
                      : CoreStrings.WorldMapMarkResourceOil_Tooltip;
        }
    }
}