namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelCharacterCustomizationControl : BaseViewModel
    {
        private static readonly Vector2Ushort HeadPreviewSpriteTextureSize = new(224, 224);

        private readonly Action<(CharacterHumanFaceStyle style, bool isMale)> callbackClose;

        private readonly ViewModelFaceStyleCustomizer faceStyleCustomizerFemale;

        private readonly ViewModelFaceStyleCustomizer faceStyleCustomizerMale;

        private ViewModelFaceStyleCustomizer currentFaceStyleCustomizer;

        private bool isMale;

        private byte lastImageLoadingId;

        private TextureBrush previewImageBrush;

        public ViewModelCharacterCustomizationControl(
            Action<(CharacterHumanFaceStyle style, bool isMale)> callbackClose)
        {
            this.callbackClose = callbackClose;

            this.CommandSave = new ActionCommand(this.ExecuteCommandSave);
            this.CommandCancel = new ActionCommand(this.ExecuteCommandCancel);
            this.CommandRandom = new ActionCommand(this.ExecuteCommandRandom);

            this.faceStyleCustomizerMale = new ViewModelFaceStyleCustomizer(isMale: true,
                                                                            onStyleSet: this.OnStyleSet);
            this.faceStyleCustomizerFemale = new ViewModelFaceStyleCustomizer(isMale: false,
                                                                              onStyleSet: this.OnStyleSet);

            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var publicState = PlayerCharacter.GetPublicState(currentPlayerCharacter);
            var privateState = PlayerCharacter.GetPrivateState(currentPlayerCharacter);
            this.isMale = publicState.IsMale;
            this.CanCancel = privateState.IsAppearanceSelected;

            this.currentFaceStyleCustomizer = this.isMale
                                                  ? this.faceStyleCustomizerMale
                                                  : this.faceStyleCustomizerFemale;

            if (privateState.IsAppearanceSelected)
            {
                this.currentFaceStyleCustomizer.CurrentStyle = publicState.FaceStyle;
            }
            else
            {
                this.currentFaceStyleCustomizer.GenerateRandomFace();
            }
        }

        public bool CanCancel { get; }

        public BaseCommand CommandCancel { get; }

        public BaseCommand CommandRandom { get; }

        public BaseCommand CommandRandomUndo
            => new ActionCommand(this.ExecuteCommandRandomUndo);

        public BaseCommand CommandSave { get; }

        public ViewModelFaceStyleCustomizer CurrentFaceStyleCustomizer
        {
            get => this.currentFaceStyleCustomizer;
            private set
            {
                if (this.currentFaceStyleCustomizer == value)
                {
                    return;
                }

                if (value is not null
                    && !value.IsStyleSet)
                {
                    value.GenerateRandomFace();
                }

                this.currentFaceStyleCustomizer = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsRandomUndoAvailable));
                this.OnStyleSet(value);
            }
        }

        public bool IsFemale
        {
            get => !this.isMale;
            set => this.IsMale = !value;
        }

        public bool IsMale
        {
            get => this.isMale;
            set
            {
                if (this.isMale == value)
                {
                    return;
                }

                this.isMale = value;
                this.NotifyThisPropertyChanged();

                this.CurrentFaceStyleCustomizer = this.isMale
                                                      ? this.faceStyleCustomizerMale
                                                      : this.faceStyleCustomizerFemale;
            }
        }

        public bool IsRandomUndoAvailable => this.currentFaceStyleCustomizer.IsRandomUndoAvailable;

        public Brush PreviewImage
        {
            get => this.previewImageBrush;
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
                this.NotifyPropertyChanged(nameof(this.IsFemale));
            }
        }

        public (CharacterHumanFaceStyle CurrentStyle, bool isMale) GetCurrentSelectedStyle()
        {
            return (this.CurrentFaceStyleCustomizer.CurrentStyle, this.isMale);
        }

        protected override void DisposeViewModel()
        {
            this.PreviewImage = null;

            this.faceStyleCustomizerMale.Dispose();
            this.faceStyleCustomizerFemale.Dispose();

            base.DisposeViewModel();
        }

        private void ExecuteCommandCancel()
        {
            this.callbackClose(default);
        }

        private void ExecuteCommandRandom()
        {
            this.currentFaceStyleCustomizer.GenerateRandomFace();
            this.NotifyPropertyChanged(nameof(this.IsRandomUndoAvailable));
        }

        private void ExecuteCommandRandomUndo()
        {
            this.currentFaceStyleCustomizer.UndoRandomFace();
            this.NotifyPropertyChanged(nameof(this.IsRandomUndoAvailable));
        }

        private void ExecuteCommandSave()
        {
            this.callbackClose(this.GetCurrentSelectedStyle());
        }

        private void OnStyleSet(ViewModelFaceStyleCustomizer viewModelFaceStyleCustomizer)
        {
            if (viewModelFaceStyleCustomizer == this.currentFaceStyleCustomizer)
            {
                this.RegeneratePreviewImage();
            }
        }

        private async void RegeneratePreviewImage()
        {
            var headSpriteData = new CharacterHeadSpriteData(
                faceStyle: this.CurrentFaceStyleCustomizer.CurrentStyle,
                headEquipmentItem: null,
                headEquipmentItemProto: null,
                skeletonResource: null);

            var imageLoadingId = ++this.lastImageLoadingId;

            var textureBrush = Client.UI.GetTextureBrush(
                new ProceduralTexture(
                    $"Character style {RandomHelper.Next()}-{Api.Client.Core.ClientFrameNumber}",
                    request => ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                        data: headSpriteData,
                        request: request,
                        isMale: this.IsMale,
                        headSpriteType: ClientCharacterHeadSpriteComposer.HeadSpriteType.Front,
                        isPreview: true,
                        customTextureSize: HeadPreviewSpriteTextureSize,
                        spriteQualityOffset: -100),
                    isUseCache: false,
                    isTransparent: true));

            // flip horizontally
            //textureBrush.RelativeTransform = new ScaleTransform(scaleX: -1, scaleY: 1, centerX: 0.5, centerY: 0.5);
            await textureBrush.WaitLoaded();

            if (imageLoadingId != this.lastImageLoadingId)
            {
                // too late - there is another image loading on the way
                textureBrush.DestroyImmediately();
                return;
            }

            this.PreviewImage = textureBrush;
        }
    }
}