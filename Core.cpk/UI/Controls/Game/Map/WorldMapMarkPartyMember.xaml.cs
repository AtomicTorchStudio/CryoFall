namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkPartyMember : BaseUserControl
    {
        private readonly string partyMemberName;

        private ViewModelPartyMember viewModel;

        public WorldMapMarkPartyMember(string partyMemberName)
        {
            this.partyMemberName = partyMemberName;
        }

        public WorldMapMarkPartyMember()
        {
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelPartyMember(this.partyMemberName,
                                                      commandRemove: null,
                                                      Visibility.Collapsed);
            this.DataContext = this.viewModel;

            this.UpdateLayout();
            var textBlock = this.GetByName<FrameworkElement>("NameGrid");
            Canvas.SetLeft(textBlock, -textBlock.ActualWidth / 2);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}