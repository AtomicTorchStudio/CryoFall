namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterCustomizationControl : BaseUserControl
    {
        public Action<(CharacterHumanFaceStyle style, bool isMale)> CallbackClose;

        private ViewModelCharacterCustomizationControl viewModel;

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelCharacterCustomizationControl(callbackClose: this.InvokeClose);
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
        }

        private void InvokeClose((CharacterHumanFaceStyle style, bool isMale) result)
        {
            this.CallbackClose(result);
        }
    }
}