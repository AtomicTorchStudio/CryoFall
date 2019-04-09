namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// In developer mode player can access the console and debug tools overlay.
    /// </summary>
    public class GeneralOptionDeveloperMode : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public const string Dialog_Button = "Enable anyway";

        public const string Dialog_Text =
            @"Important! You are about to enable developer mode.
              [br][br]
              Using certain developer mode features can break your game.
              [br][br]
              Unless you really know what you are doing, you are strongly advised NOT to do this.
              [br]";

        private static GeneralOptionDeveloperMode instance;

        /// <summary>
        /// Returns true if developer mode is active.
        /// </summary>
        public static bool IsEnabled => instance.CurrentValue;

        // developer mode is enabled by default in Editor
        public override bool Default => Api.IsEditor;

        public override string Name => "Developer mode";

        public override IProtoOption OrderAfterOption => GetProtoEntity<GeneralOptionDisplayObjectInteractionTooltip>();

        public override bool ValueProvider { get; set; } // do nothing

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            var isDeveloperMode = this.CurrentValue;
            if (fromUi && isDeveloperMode)
            {
                // Tell user it's dangerous!
                DialogWindow.ShowDialog(
                    title: this.Name,
                    text: Dialog_Text,
                    textAlignment: TextAlignment.Left,
                    okText: Dialog_Button,
                    cancelText: CoreStrings.Button_Cancel,
                    okAction: () => { },
                    cancelAction: () => { this.CurrentValue = false; },
                    autoWidth: false,
                    focusOnCancelButton: true);
            }
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }
    }
}