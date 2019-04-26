namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.Systems.DayNightSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class NicknameDisplay : BaseUserControl
    {
        private ICharacter character;

        private ViewModelNicknameDisplay viewModel;

        public void Setup(ICharacter character, bool isOnline)
        {
            this.character = character;
            this.viewModel = new ViewModelNicknameDisplay(character, isOnline);
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel;
            ClientComponentUpdateHelper.UpdateCallback += this.RefreshVisibilityOnly;
            this.RefreshVisibilityOnly();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;

            this.viewModel?.Dispose();
            this.viewModel = null;

            ClientComponentUpdateHelper.UpdateCallback -= this.RefreshVisibilityOnly;
        }
  
        private void RefreshVisibilityOnly()
        {
            if (!ClientTimeOfDayVisibilityHelper.ClientIsObservable(this.character))
            {
                this.Visibility = Visibility.Hidden;
                return;
            }

            this.Visibility = Visibility.Visible;
        }
    }
}