namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ViewModelRebindKeyWindow : BaseViewModel
    {
        public const string MessageCannotBindToKey = "You cannot bind to this key:[br][b]{0}[/b]";

        public const string MessageFormatKeyAlreadyBound =
            @"The pressed key is already bound to button:
              [br][b]{0}[/b]
              [br]
              [br]Press this key again to rebind the key
              [br]or press another key.";

        public const string MessagePressToBind = "Please press a key to bind or press Escape to cancel.";

        private readonly IWrappedButton buttonToRebind;

        private readonly bool isSecondaryKey;

        private readonly Action onClose;

        private ClientInputContext clientInputContext;

        private InputKey lastPressedKey;

        private ViewModelButtonMappingControl viewModelMapping;

        public ViewModelRebindKeyWindow(
            ViewModelButtonMappingControl viewModelMapping,
            bool isSecondaryKey,
            Action onClose)
        {
            this.viewModelMapping = viewModelMapping;
            this.buttonToRebind = viewModelMapping.Button;

            this.onClose = onClose;
            this.isSecondaryKey = isSecondaryKey;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.clientInputContext = ClientInputContext.Start("Key binding")
                                                        .HandleAll(this.InputCallback);

            Instance = this;
        }

        public static ViewModelRebindKeyWindow Instance { get; private set; }

        public string ButtonTitle => this.viewModelMapping.ButtonInfo.Title;

        public string Message { get; private set; } = MessagePressToBind;

        protected override void DisposeViewModel()
        {
            this.viewModelMapping = null;
            base.DisposeViewModel();
            this.clientInputContext.Stop();
            this.clientInputContext = null;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void HandleKeyDown(InputKey key)
        {
            switch (key)
            {
                case InputKey.Escape:
                    // binding cancelled
                    this.onClose();
                    return;

                //case InputKey.OemTilde:
                case InputKey.Control:
                case InputKey.Alt:
                    this.Message = string.Format(MessageCannotBindToKey,
                                                 InputKeyNameHelper.GetKeyText(key,
                                                                               returnPlaceholderIfNone: false));
                    return;
            }

            if (this.lastPressedKey != key)
            {
                // player pressed another key - verify if there are collisions with other mapped buttons
                this.lastPressedKey = key;

                var buttons = ClientInputManager.GetButtonForKey(
                    key,
                    this.viewModelMapping.ButtonInfo.Category);

                foreach (var button in buttons)
                {
                    if (button.Equals(this.buttonToRebind))
                    {
                        // pressed a key already bound to this button - allow rebinding
                        break;
                    }

                    // pressed a different key
                    this.Message = string.Format(MessageFormatKeyAlreadyBound,
                                                 ClientInputManager.GetButtonInfo(button).Title);
                    return;
                }
            }

            // do remapping
            var mapping = ClientInputManager.GetMappingForAbstractButton(this.buttonToRebind);
            if (this.isSecondaryKey)
            {
                var secondaryKey = key;
                var primaryKey = mapping.PrimaryKey != secondaryKey ? mapping.PrimaryKey : InputKey.None;
                mapping = new ButtonMapping(primaryKey, secondaryKey);
            }
            else
            {
                var primaryKey = key;
                var secondaryKey = mapping.SecondaryKey != primaryKey ? mapping.SecondaryKey : InputKey.None;
                mapping = new ButtonMapping(primaryKey, secondaryKey);
            }

            ClientInputManager.SetAbstractButtonMapping(this.buttonToRebind, mapping);

            Api.GetProtoEntity<ControlsOptionsCategory>().NotifyModified();

            this.onClose();
        }

        private void InputCallback()
        {
            var keyDown = Client.Input
                                .GetAllKeysDown(evenIfHandled: true)
                                .FirstOrDefault();

            if (keyDown != InputKey.None)
            {
                this.HandleKeyDown(keyDown);
            }

            ClientInputManager.ConsumeAllButtons();
        }
    }
}