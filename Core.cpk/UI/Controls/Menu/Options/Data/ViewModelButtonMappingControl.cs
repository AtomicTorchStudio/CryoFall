namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelButtonMappingControl : BaseViewModel
    {
        public readonly IWrappedButton Button;

        public readonly ButtonInfoAttribute ButtonInfo;

        public ViewModelButtonMappingControl(IWrappedButton button, ButtonInfoAttribute buttonInfo)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.CommandBindKey = new ActionCommandWithParameter(
                param => this.ExecuteCommandBindKey("Secondary".Equals(param)));
            this.Button = button;
            this.ButtonInfo = buttonInfo;
            this.UpdateMapping();

            ClientInputManager.ButtonKeyMappingUpdated += this.ButtonKeyMappingUpdatedHandler;
        }

        public ViewModelButtonMappingControl()
        {
        }

        public BaseCommand CommandBindKey { get; }

        public string PrimaryKeyText { get; private set; }

        public string SecondaryKeyText { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientInputManager.ButtonKeyMappingUpdated -= this.ButtonKeyMappingUpdatedHandler;
        }

        private void ButtonKeyMappingUpdatedHandler(IWrappedButton obj)
        {
            if (this.Button.Equals(obj))
            {
                this.UpdateMapping();
            }
        }

        private void ExecuteCommandBindKey(bool isSecondaryKey)
        {
            var editWindow = new RebindKeyWindow();
            editWindow.Setup(this, isSecondaryKey);
            Api.Client.UI.LayoutRootChildren.Add(editWindow);
        }

        private void UpdateMapping()
        {
            var mapping = ClientInputManager.GetMappingForAbstractButton(this.Button);
            this.PrimaryKeyText = InputKeyNameHelper.GetKeyText(mapping.PrimaryKey);
            this.SecondaryKeyText = InputKeyNameHelper.GetKeyText(mapping.SecondaryKey);
        }
    }
}