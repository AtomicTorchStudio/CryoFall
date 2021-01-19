namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelFaceStyleCustomizer : BaseViewModel
    {
        // face bottom part title
        public const string TitleBottom = "Bottom";

        // face preset title (every face has different set of top and bottom options)
        public const string TitleFace = "Face";

        public const string TitleHair = "Hair";

        public const string TitleHairColor = "Hair color";

        public const string TitleSkinTone = "Skin tone";

        // face top part title
        public const string TitleTop = "Top";

        private readonly SharedCharacterFaceStylesProvider faceStylesProvider;

        private readonly Action<ViewModelFaceStyleCustomizer> onStyleSet;

        private readonly Stack<CharacterHumanFaceStyle> previousStyles
            = new();

        private readonly ViewModelCharacterStyleSetting settingBottom;

        private readonly ViewModelCharacterStyleSetting settingFace;

        private readonly ViewModelCharacterStyleSetting settingHair;

        private readonly ViewModelCharacterStyleSetting settingHairColor;

        private readonly ViewModelCharacterStyleSetting settingSkinTone;

        private readonly ViewModelCharacterStyleSetting settingTop;

        private CharacterHumanFaceStyle? currentStyle;

        private bool isHandlersSuppressed;

        public ViewModelFaceStyleCustomizer(bool isMale, Action<ViewModelFaceStyleCustomizer> onStyleSet)
        {
            this.onStyleSet = onStyleSet;
            this.faceStylesProvider = SharedCharacterFaceStylesProvider.GetForGender(isMale);

            this.settingFace = new ViewModelCharacterStyleSetting(
                TitleFace,
                maxValue: this.faceStylesProvider.GetFacesCount() - 1,
                valueChangedCallback: this.SettingFaceValueChangedCallback);

            this.settingTop = new ViewModelCharacterStyleSetting(
                TitleTop,
                maxValue: 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingBottom = new ViewModelCharacterStyleSetting(
                TitleBottom,
                maxValue: 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingHair = new ViewModelCharacterStyleSetting(
                TitleHair,
                maxValue: this.faceStylesProvider.GetHairCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingSkinTone = new ViewModelCharacterStyleSetting(
                TitleSkinTone,
                maxValue: this.faceStylesProvider.GetSkinToneCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.settingHairColor = new ViewModelCharacterStyleSetting(
                TitleHairColor,
                maxValue: this.faceStylesProvider.GetHairColorCount() - 1,
                valueChangedCallback: this.SettingValueChangedCallback);

            this.StyleSettings = new ObservableCollection<ViewModelCharacterStyleSetting>
            {
                this.settingFace,
                this.settingSkinTone,
                this.settingTop,
                this.settingHair,
                this.settingBottom,
                this.settingHairColor
            };
        }

        public CharacterHumanFaceStyle CurrentStyle
        {
            get => this.currentStyle ?? throw new Exception("Style not set");
            set
            {
                if (this.currentStyle.HasValue
                    && this.currentStyle.Value.Equals(value))
                {
                    return;
                }

                this.currentStyle = value;

                this.isHandlersSuppressed = true;
                var face = this.faceStylesProvider.GetFace(value.FaceId);
                this.settingFace.Value = face.Index;
                this.UpdateSettingsViewModels();
                this.settingTop.Value = (ushort)Array.IndexOf(face.TopIds,       value.TopId);
                this.settingBottom.Value = (ushort)Array.IndexOf(face.BottomIds, value.BottomId);
                this.settingHair.Value = this.faceStylesProvider.GetHairStyleIndex(value.HairId);
                this.settingSkinTone.Value = this.faceStylesProvider.GetSkinToneIndex(value.SkinToneId);
                this.settingHairColor.Value = this.faceStylesProvider.GetHairColorIndex(value.HairColorId);
                this.isHandlersSuppressed = false;

                this.onStyleSet(this);
            }
        }

        public bool IsRandomUndoAvailable => this.previousStyles.Count > 0;

        public bool IsStyleSet => this.currentStyle.HasValue;

        public ObservableCollection<ViewModelCharacterStyleSetting> StyleSettings { get; }

        public void GenerateRandomFace()
        {
            if (this.currentStyle.HasValue)
            {
                this.previousStyles.Push(this.currentStyle.Value);
            }

            this.CurrentStyle = this.faceStylesProvider.GenerateRandomFace();
        }

        public void UndoRandomFace()
        {
            if (this.IsRandomUndoAvailable)
            {
                this.CurrentStyle = this.previousStyles.Pop();
            }
        }

        private void RefreshCurrentStyle()
        {
            var faceIndex = this.settingFace.Value;
            var topIndex = this.settingTop.Value;
            var bottomIndex = this.settingBottom.Value;
            var hairIndex = this.settingHair.Value;
            var skinToneIndex = this.settingSkinTone.Value;
            var hairColorIndex = this.settingHairColor.Value;

            var face = this.faceStylesProvider.GetFace((ushort)faceIndex);
            var topId = face.TopIds[topIndex];
            var bottomId = face.BottomIds[bottomIndex];
            var hairId = this.faceStylesProvider.GetHairStyleByIndex((ushort)hairIndex);
            var skinToneId = this.faceStylesProvider.GetSkinToneByIndex((ushort)skinToneIndex);
            var hairColorId = this.faceStylesProvider.GetHairColorByIndex((ushort)hairColorIndex);

            this.CurrentStyle = new CharacterHumanFaceStyle(face.Id,
                                                            topId,
                                                            bottomId,
                                                            hairId,
                                                            skinToneId,
                                                            hairColorId);
        }

        private void SettingFaceValueChangedCallback()
        {
            if (this.isHandlersSuppressed)
            {
                return;
            }

            this.UpdateSettingsViewModels();
            this.RefreshCurrentStyle();
        }

        private void SettingValueChangedCallback()
        {
            if (!this.isHandlersSuppressed)
            {
                this.RefreshCurrentStyle();
            }
        }

        private void UpdateSettingsViewModels()
        {
            var face = this.faceStylesProvider.GetFace((ushort)this.settingFace.Value);
            this.settingTop.MaxValue = face.TopIds.Length - 1;
            this.settingBottom.MaxValue = face.BottomIds.Length - 1;
        }
    }
}