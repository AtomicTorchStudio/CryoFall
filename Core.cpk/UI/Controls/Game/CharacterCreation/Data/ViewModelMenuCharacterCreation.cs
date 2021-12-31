namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterCreation;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuCharacterCreation : BaseViewModel
    {
        private const double ButtonNextEnableDelay = 1.0;

        private readonly Action closeCallback;

        [ViewModelNotAutoDisposeField]
        private readonly CharacterCustomizationControl characterCustomizationControl;

        private readonly bool isAppearanceSelected;

        private int lastNextButtonEnableRequestId;

        public ViewModelMenuCharacterCreation(
            CharacterCustomizationControl characterCustomizationControl,
            Action closeCallback)
        {
            this.closeCallback = closeCallback;
            this.characterCustomizationControl = characterCustomizationControl;

            this.AllOrigins = Api.FindProtoEntities<ProtoCharacterOrigin>()
                                 .OrderBy(p => p.ShortId)
                                 .Select(p => new DataEntryCharacterOrigin(p))
                                 .ToArray();

            this.SelectedOrigin = this.AllOrigins.FirstOrDefault();

            this.ActivateNextButtonAfterDelay();

            this.isAppearanceSelected = ClientCurrentCharacterHelper.PrivateState.IsAppearanceSelected;
            if (this.isAppearanceSelected)
            {
                // skip appearance selection
                this.ScreenIndex = 1;
            }
        }

        public IReadOnlyList<DataEntryCharacterOrigin> AllOrigins { get; }

        public BaseCommand CommandNextScreen => new ActionCommand(this.ExecuteCommandNextScreen);

        public BaseCommand CommandPreviousScreen => new ActionCommand(this.ExecuteCommandPreviousScreen);

        public bool IsButtonNextEnabled { get; private set; }

        public bool IsButtonPreviousVisible
            => this.ScreenIndex > 0
               && !this.isAppearanceSelected;

        public int ScreenIndex { get; private set; }

        public DataEntryCharacterOrigin SelectedOrigin { get; set; }

        private void ActivateNextButtonAfterDelay()
        {
            this.IsButtonNextEnabled = false;
            var requestId = ++this.lastNextButtonEnableRequestId;
            ClientTimersSystem.AddAction(ButtonNextEnableDelay,
                                         () =>
                                         {
                                             if (requestId == this.lastNextButtonEnableRequestId)
                                             {
                                                 this.IsButtonNextEnabled = true;
                                             }
                                         });
        }

        private async void CreateCharacter()
        {
            (CharacterHumanFaceStyle style, bool isMale) selectedStyle
                = this.characterCustomizationControl.GetSelectedStyle();

            // show splash screen
            LoadingSplashScreenManager.Show("Character created", displayStructureInfos: false);
            await LoadingSplashScreenManager.WaitShownAsync();

            // select the style only now, when the loading splash is displayed,
            // so there is no stuttering of the loading splash screen animation
            CharacterStyleSystem.ClientChangeStyle(selectedStyle.style, selectedStyle.isMale);
            CharacterCreationSystem.ClientSelectOrigin(this.SelectedOrigin.ProtoCharacterOrigin);

            this.closeCallback();

            // allow hiding after a short delay (it will still check whether everything is loaded)
            ClientTimersSystem.AddAction(
                delaySeconds: 1.0 + Math.Min(1, Api.Client.CurrentGame.PingGameSeconds),
                LoadingSplashScreenManager.Hide);
        }

        private void ExecuteCommandNextScreen()
        {
            if (this.ScreenIndex < 1)
            {
                this.ScreenIndex++;
                this.ActivateNextButtonAfterDelay();
                this.NotifyPropertyChanged(nameof(this.IsButtonPreviousVisible));
            }
            else
            {
                // character creation finished
                this.CreateCharacter();
            }
        }

        private void ExecuteCommandPreviousScreen()
        {
            if (this.ScreenIndex > 0)
            {
                this.ScreenIndex--;
                this.NotifyPropertyChanged(nameof(this.IsButtonPreviousVisible));
            }
        }

        public readonly struct DataEntryCharacterOrigin
        {
            public DataEntryCharacterOrigin(ProtoCharacterOrigin protoCharacterOrigin)
            {
                this.ProtoCharacterOrigin = protoCharacterOrigin;
            }

            public string Description => this.ProtoCharacterOrigin.Description;

            public Brush Icon => Api.Client.UI.GetTextureBrush(this.ProtoCharacterOrigin.Icon);

            public ProtoCharacterOrigin ProtoCharacterOrigin { get; }

            public IReadOnlyStatsDictionary StatsDictionary => this.ProtoCharacterOrigin.Effects;

            public string Title => this.ProtoCharacterOrigin.Name;
        }
    }
}