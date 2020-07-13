namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelFeaturesSlideshow : BaseViewModel
    {
        public const string ButtonStartTheGame = "Start the game";

        private const double ButtonNextEnableDelay = 1.0;

        private int currentEntryIndex;

        private ClientInputContext inputContext;

        private int lastNextButtonEnableRequestId;

        private int maxDisplayedEntryIndex;

        public ViewModelFeaturesSlideshow()
        {
            this.inputContext = ClientInputContext
                                .Start(nameof(ViewModelFeaturesSlideshow))
                                .HandleAll(
                                    () =>
                                    {
                                        if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose)
                                            && (this.IsCloseButtonVisible
                                                // for debug purposes allow to skip the slideshow
                                                // in a debug build or when the Shift key is held
                                                || Api.Shared.IsDebug
                                                || Api.Client.Input.IsKeyHeld(InputKey.Shift)))
                                        {
                                            this.ExecuteCommandClose();
                                        }

                                        ClientInputManager.ConsumeAllButtons();
                                    });

            this.Entries = FeaturesSlideshowEntries
                           .Entries
                           .Select(e => new ViewModelFeaturesSlideshowEntry(
                                       e.Title,
                                       e.Description,
                                       new TextureResource(e.TextureImagePath)))
                           .ToArray();

            this.IsCloseButtonVisible = FeaturesSlideshow.IsSlideShowFinishedAtLeastOnce;

            if (this.IsCloseButtonVisible)
            {
                this.maxDisplayedEntryIndex = this.Entries.Count - 1;
            }
            else
            {
                this.maxDisplayedEntryIndex = -1;
            }

            this.RefreshNavigationButtons();
        }

        public string ButtonNextText => this.currentEntryIndex + 1 < this.Entries.Count
                                            ? CoreStrings.Button_Next
                                            : ButtonStartTheGame;

        public BaseCommand CommandClose
            => new ActionCommand(this.ExecuteCommandClose);

        public BaseCommand CommandNext
            => new ActionCommand(() =>
                                 {
                                     if (this.currentEntryIndex + 1 >= this.Entries.Count)
                                     {
                                         this.ExecuteCommandClose();
                                         return;
                                     }

                                     this.CurrentEntryIndex++;
                                 });

        public BaseCommand CommandPrevious
            => new ActionCommand(() => this.CurrentEntryIndex--);

        public ViewModelFeaturesSlideshowEntry CurrentEntry
            => this.Entries[this.currentEntryIndex];

        public int CurrentEntryIndex
        {
            get => this.currentEntryIndex;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value >= this.Entries.Count)
                {
                    value = this.Entries.Count - 1;
                }

                if (value == this.currentEntryIndex)
                {
                    return;
                }

                this.currentEntryIndex = value;

                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.CurrentEntry));
                this.NotifyPropertyChanged(nameof(this.ButtonNextText));
                this.NotifyPropertyChanged(nameof(this.IsButtonPreviousVisible));

                this.RefreshNavigationButtons();
            }
        }

        public IReadOnlyList<ViewModelFeaturesSlideshowEntry> Entries { get; }

        public bool IsButtonNextEnabled { get; private set; }

        public bool IsButtonPreviousEnabled { get; private set; }

        public bool IsButtonPreviousVisible => this.currentEntryIndex > 0;

        public bool IsCloseButtonVisible { get; }

        protected override void DisposeViewModel()
        {
            this.inputContext.Stop();
            this.inputContext = null;
            base.DisposeViewModel();
        }

        private void ExecuteCommandClose()
        {
            FeaturesSlideshow.IsSlideShowFinishedAtLeastOnce = true;
            FeaturesSlideshow.IsDisplayed = false;
        }

        private void RefreshIsButtonNextEnabled(double delay)
        {
            this.IsButtonNextEnabled = false;

            var requestId = ++this.lastNextButtonEnableRequestId;
            ClientTimersSystem.AddAction(delay,
                                         () =>
                                         {
                                             if (requestId == this.lastNextButtonEnableRequestId)
                                             {
                                                 this.IsButtonNextEnabled = true;
                                             }
                                         });
        }

        private void RefreshNavigationButtons()
        {
            var requestId = ++this.lastNextButtonEnableRequestId;
            if (this.currentEntryIndex <= this.maxDisplayedEntryIndex)
            {
                this.IsButtonPreviousEnabled = this.IsButtonNextEnabled = true;
                return;
            }

            this.maxDisplayedEntryIndex = this.currentEntryIndex;
            this.IsButtonPreviousEnabled = this.IsButtonNextEnabled = false;

            ClientTimersSystem.AddAction(ButtonNextEnableDelay,
                                         () =>
                                         {
                                             if (requestId == this.lastNextButtonEnableRequestId)
                                             {
                                                 this.IsButtonPreviousEnabled = this.IsButtonNextEnabled = true;
                                             }
                                         });
        }
    }
}