namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using static Systems.Faction.SharedFactionEmblemProvider;

    public class ViewModelFactionEmblemEditor : BaseViewModel
    {
        public const string TitleBackground = "Background";

        // Background color 1
        public const string TitleBackgroundColor1 = "Bg. color 1";

        // Background color 2
        public const string TitleBackgroundColor2 = "Bg. color 2";

        public const string TitleEmblem = "Emblem";

        public const string TitleEmblemColor = "Emblem color";

        public const string TitleShape = "Shape";

        private readonly Stack<FactionEmblem> previousEmblems
            = new();

        private readonly ViewModelCharacterStyleSetting settingBackground;

        private readonly ViewModelColorPickerFromPreset settingBackgroundColor1;

        private readonly ViewModelColorPickerFromPreset settingBackgroundColor2;

        private readonly ViewModelCharacterStyleSetting settingForeground;

        private readonly ViewModelColorPickerFromPreset settingForegroundColor;

        private readonly ViewModelCharacterStyleSetting settingShapeMask;

        private FactionEmblem currentEmblem;

        private bool isHandlersSuppressed;

        private bool isPreviewImageRegenerationScheduled;

        private int lastImageLoadingId;

        private int lastLoadedImageId = -1;

        private TextureBrush previewImageBrush;

        public ViewModelFactionEmblemEditor(FactionEmblem? emblem = null)
        {
            this.settingForeground = new ViewModelCharacterStyleSetting(
                TitleEmblem,
                maxValue: GetForegroundCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingShapeMask = new ViewModelCharacterStyleSetting(
                TitleShape,
                maxValue: GetShapeMaskCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingBackground = new ViewModelCharacterStyleSetting(
                TitleBackground,
                maxValue: GetBackgroundCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            var supportedColorsForeground = SupportedColorsForeground.Select(c => (Color)c)
                                                                     .ToArray();

            var supportedColorsBackground = SupportedColorsBackground.Select(c => (Color)c)
                                                                     .ToArray();

            this.settingForegroundColor = new ViewModelColorPickerFromPreset(
                TitleEmblemColor,
                supportedColorsForeground,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingBackgroundColor1 = new ViewModelColorPickerFromPreset(
                TitleBackgroundColor1,
                supportedColorsBackground,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingBackgroundColor2 = new ViewModelColorPickerFromPreset(
                TitleBackgroundColor2,
                supportedColorsBackground,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.EmblemSettings = new ObservableCollection<BaseViewModel>
            {
                this.settingForeground,
                this.settingShapeMask,
                this.settingBackground,
                this.settingForegroundColor,
                this.settingBackgroundColor1,
                this.settingBackgroundColor2
            };

            this.CurrentEmblem = emblem ?? GenerateRandomEmblem();
        }

        public BaseCommand CommandRandom
            => new ActionCommand(this.ExecuteCommandRandom);

        public BaseCommand CommandRandomUndo
            => new ActionCommand(this.ExecuteCommandRandomUndo);

        public FactionEmblem CurrentEmblem
        {
            get => this.currentEmblem;
            set
            {
                if (this.currentEmblem.Equals(value))
                {
                    return;
                }

                this.currentEmblem = value;

                this.isHandlersSuppressed = true;
                this.settingForeground.Value = GetForegroundStyleIndex(value.ForegroundId);
                this.settingBackground.Value = GetBackgroundStyleIndex(value.BackgroundId);
                this.settingShapeMask.Value = GetShapeMaskStyleIndex(value.ShapeMaskId);
                this.settingForegroundColor.Color = value.ColorForeground;
                this.settingBackgroundColor1.Color = value.ColorBackground1;
                this.settingBackgroundColor2.Color = value.ColorBackground2;
                this.isHandlersSuppressed = false;

                this.NotifyThisPropertyChanged();
                this.ScheduleRegeneratePreviewImage();
            }
        }

        public ObservableCollection<BaseViewModel> EmblemSettings { get; }

        public bool IsRandomUndoAvailable => this.previousEmblems.Count > 0;

        public Brush PreviewImage
        {
            get => IsDesignTime ? Brushes.Red : (Brush)this.previewImageBrush;
            set
            {
                if (this.IsDisposed)
                {
                    (value as TextureBrush)?.DestroyImmediately();
                    value = null;
                }

                if (this.previewImageBrush == value)
                {
                    return;
                }

                this.previewImageBrush?.DestroyImmediately();
                this.previewImageBrush = (TextureBrush)value;

                this.NotifyThisPropertyChanged();
            }
        }

        private void ExecuteCommandRandom()
        {
            this.previousEmblems.Push(this.CurrentEmblem);
            this.CurrentEmblem = GenerateRandomEmblem();
            this.NotifyPropertyChanged(nameof(this.IsRandomUndoAvailable));
        }

        private void ExecuteCommandRandomUndo()
        {
            if (!this.IsRandomUndoAvailable)
            {
                return;
            }

            this.CurrentEmblem = this.previousEmblems.Pop();
            this.NotifyPropertyChanged(nameof(this.IsRandomUndoAvailable));
        }

        private void RefreshCurrentStyle()
        {
            var backgroundIndex = this.settingBackground.Value;
            var backgroundId = GetBackgroundStyleByIndex((ushort)backgroundIndex);

            var shapeMaskIndex = this.settingShapeMask.Value;
            var shapeMaskId = GetShapeMaskStyleByIndex((ushort)shapeMaskIndex);

            var foregroundIndex = this.settingForeground.Value;
            var foregroundId = GetForegroundStyleByIndex((ushort)foregroundIndex);

            var colorForeground = this.settingForegroundColor.Color;
            var colorBackground1 = this.settingBackgroundColor1.Color;
            var colorBackground2 = this.settingBackgroundColor2.Color;

            this.CurrentEmblem = new FactionEmblem(foregroundId,
                                                   backgroundId,
                                                   shapeMaskId,
                                                   colorForeground,
                                                   colorBackground1,
                                                   colorBackground2);
        }

        private async void RegeneratePreviewImage()
        {
            this.isPreviewImageRegenerationScheduled = false;
            var imageLoadingId = ++this.lastImageLoadingId;

            var textureBrush = Client.UI.GetTextureBrush(
                ClientFactionEmblemTextureProvider.GetEmblemTexture(this.currentEmblem, useCache: false));

            await textureBrush.WaitLoaded();

            if (imageLoadingId < this.lastLoadedImageId)
            {
                // too late - a more recently requested image was loaded first
                textureBrush.DestroyImmediately();
                return;
            }

            this.lastLoadedImageId = imageLoadingId;
            this.PreviewImage = textureBrush;
        }

        private void ScheduleRegeneratePreviewImage()
        {
            if (this.isPreviewImageRegenerationScheduled)
            {
                return;
            }

            this.isPreviewImageRegenerationScheduled = true;
            ClientTimersSystem.AddAction(0.1,
                                         () =>
                                         {
                                             if (this.isPreviewImageRegenerationScheduled)
                                             {
                                                 this.RegeneratePreviewImage();
                                             }
                                         });
        }

        private void SettingValueChangedCallback()
        {
            if (!this.isHandlersSuppressed)
            {
                this.RefreshCurrentStyle();
            }
        }
    }
}