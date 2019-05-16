namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class VideoOptionSpriteResolution
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionSpriteResolution.Mode>
    {
        public const string DialogRestart_Message =
            @"The sprite resolution video option has been changed.
              [br]The game requires a restart in order to apply the changes.
              [br]Please restart the game manually.";

        public const string DialogRestart_Title = "Restart required";

        public const string DialogTooHighSetting_Button = "Continue anyway";

        public const string DialogTooHighSetting_MessageFormat =
            @"You are trying to select a sprite resolution mode that is higher than the recommended mode.
              [br]
              [*] Recommended mode: [b]{0}[/b]
              [*] Selected mode: [b]{1}[/b]
              [br]
              [br]The selected mode requires more VRAM capacity than your GPU has. This could lead to image stuttering and low framerate.";

        public const string DialogTooHighSetting_Title = "Too high setting selected";

        protected internal static readonly IRenderingClientService Rendering
            = Api.IsClient
                  ? Api.Client.Rendering
                  : null;

        [NotPersistent]
        public enum Mode : byte
        {
            [Description("Ultra 4K")]
            Ultra = 1,

            [Description("High")]
            High = 2,

            [Description("Low (blurry, inaccurate)")]
            Low = 4
        }

        public override Mode DefaultEnumValue
            => GetRecommendedMode();

        public override string Name => "Sprite resolution[br](needs restart)";

        public override IProtoOption OrderAfterOption
            => GetOption<VideoOptionRenderingResolutionScale>();

        public override Mode ValueProvider
        {
            get => (Mode)Rendering.SpriteQualitySizeMultiplierReverse;
            set
            {
                if (Rendering.SpriteQualitySizeMultiplierReverse == (byte)value)
                {
                    Logger.Info("Sprite resolution option is already set to: " + value);
                    return;
                }

                Logger.Important("Sprite resolution option is changed to: " + value);
                Rendering.SpriteQualitySizeMultiplierReverse = (byte)value;

                if (MainMenuOverlay.IsOptionsMenuSelected)
                {
                    DialogWindow.ShowDialog(
                        DialogRestart_Title,
                        DialogRestart_Message,
                        okText: CoreStrings.Button_Quit,
                        okAction: () => Client.Core.Quit());
                }
            }
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            if (!fromUi)
            {
                return;
            }

            var recommendedMode = GetRecommendedMode();
            var selectedMode = this.CurrentValue;
            if (recommendedMode > selectedMode)
            {
                DialogWindow.ShowDialog(
                    title: DialogTooHighSetting_Title,
                    text: string.Format(
                        DialogTooHighSetting_MessageFormat,
                        recommendedMode.GetDescription(),
                        selectedMode.GetDescription()),
                    textAlignment: TextAlignment.Left,
                    okText: DialogTooHighSetting_Button,
                    cancelText: CoreStrings.Button_Cancel,
                    okAction: () => { },
                    cancelAction: () => { this.CurrentValue = this.SavedValue; },
                    autoWidth: false,
                    focusOnCancelButton: true);
            }
        }

        private static Mode GetRecommendedMode()
        {
            var vramDedicated = Client.Rendering.GpuVramDedicatedAmount / (1024 * 1024);
            var vramDedicatedWithSharedRam = Client.Rendering.GpuVramWithSharedRamAmount / (1024 * 1024);
            var vramBudget = Client.Rendering.GpuVramBudget / (1024 * 1024);

            Mode result;
            {
                var vram = Math.Max(vramDedicated, vramBudget);
                if (vram > 1900)
                {
                    // plenty of VRAM so we can use original textures (4K)
                    result = Mode.Ultra;
                }
                else if (vram > 900
                         || vramDedicatedWithSharedRam > 900)
                {
                    // enough VRAM for high resolution textures
                    result = Mode.High;
                }
                else
                {
                    // too low VRAM and shared system RAM
                    result = Mode.Low; // low (blurry) mode
                }
            }

            Logger.Important(
                string.Format(
                    "Auto-detected Sprite Resolution mode: {1}"
                    + "{0}VRAM budget: {2} MB"
                    + "{0}VRAM dedicated: {3} MB"
                    + "{0}VRAM dedicated + shared RAM: {4} MB",
                    Environment.NewLine,
                    result,
                    vramBudget,
                    vramDedicated,
                    vramDedicatedWithSharedRam));

            return result;
        }
    }
}