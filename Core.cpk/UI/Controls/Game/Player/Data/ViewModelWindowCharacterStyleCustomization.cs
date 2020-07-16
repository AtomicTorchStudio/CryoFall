namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelWindowCharacterStyleCustomization : BaseViewModel
    {
        private static readonly Vector2Ushort HeadPreviewSpriteTextureSize = new Vector2Ushort(224, 224);

        private readonly ViewModelFaceStyleCustomizer faceStyleCustomizerFemale;

        private readonly ViewModelFaceStyleCustomizer faceStyleCustomizerMale;

        private readonly GameWindow gameWindow;

        private ViewModelFaceStyleCustomizer currentFaceStyleCustomizer;

        private bool isMale;

        private byte lastImageLoadingId;

        private TextureBrush previewImageBrush;

        public ViewModelWindowCharacterStyleCustomization()
        {
            if (!IsDesignTime)
            {
                throw new Exception("Use another constructor in the game.");
            }

            this.faceStyleCustomizerMale = new ViewModelFaceStyleCustomizer();
            this.currentFaceStyleCustomizer = this.faceStyleCustomizerFemale;
        }

        public ViewModelWindowCharacterStyleCustomization(GameWindow gameWindow)
        {
            this.gameWindow = gameWindow;

            this.CommandSave = new ActionCommand(this.ExecuteCommandSave);
            this.CommandCancel = new ActionCommand(this.ExecuteCommandCancel);
            this.CommandRandom = new ActionCommand(this.ExecuteCommandRandom);

            this.faceStyleCustomizerMale = new ViewModelFaceStyleCustomizer(isMale: true, onStyleSet: this.OnStyleSet);
            this.faceStyleCustomizerFemale =
                new ViewModelFaceStyleCustomizer(isMale: false,
                                                 onStyleSet: this.OnStyleSet);

            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var publicState = PlayerCharacter.GetPublicState(currentPlayerCharacter);
            this.isMale = publicState.IsMale;

            this.currentFaceStyleCustomizer = this.isMale
                                                  ? this.faceStyleCustomizerMale
                                                  : this.faceStyleCustomizerFemale;

            this.currentFaceStyleCustomizer.CurrentStyle = publicState.FaceStyle;
        }

        public BaseCommand CommandCancel { get; }

        public BaseCommand CommandRandom { get; }

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

                if (value != null
                    && !value.IsStyleSet)
                {
                    value.GenerateRandomFace();
                }

                this.currentFaceStyleCustomizer = value;
                this.NotifyThisPropertyChanged();
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
                this.NotifyPropertyChanged(nameof(this.IsFemale));
            }
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
            this.gameWindow.Close(DialogResult.Cancel);
        }

        private void ExecuteCommandRandom()
        {
            this.currentFaceStyleCustomizer.GenerateRandomFace();
        }

        private void ExecuteCommandSave()
        {
            CharacterStyleSystem.ClientChangeStyle(this.CurrentFaceStyleCustomizer.CurrentStyle, this.isMale);
            this.gameWindow.Close(DialogResult.OK);
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
                headEquipment: null,
                skeletonResource: null);

            var imageLoadingId = ++this.lastImageLoadingId;

            var textureBrush = Client.UI.GetTextureBrush(
                new ProceduralTexture(
                    "Character style",
                    request => ClientCharacterHeadSpriteComposer.GenerateHeadSprite(
                        data: headSpriteData,
                        request: request,
                        isMale: this.IsMale,
                        headSpriteType: ClientCharacterHeadSpriteComposer.HeadSpriteType.Front,
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