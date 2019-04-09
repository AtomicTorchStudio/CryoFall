namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientLanguages;
    using AtomicTorch.CBND.CoreMod.Languages;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuLanguageSelection : BaseViewModel
    {
        private ViewModelLanguageDefinition selectedLanguage;

        public ViewModelMenuLanguageSelection()
        {
            this.Languages = ClientLanguagesManager.AllLanguageDefinitions
                                                   .Select(l => new ViewModelLanguageDefinition(l))
                                                   .ToArray();

            //// uncomment to test scrolling (will populate the same languages multiple times)
            //for (var i = 0; i < 4; i++)
            //{
            //    this.Languages = this.Languages.Concat(this.Languages).ToArray();
            //}

            this.RefreshSelectedLanguage();

            ClientLanguagesManager.CurrentLanguageDefinitionChanged
                += this.CurrentLanguageDefinitionChangedHandler;
        }

        public BaseCommand CommandAccept
            => new ActionCommand(this.ExecuteCommandAccept);

        public IReadOnlyList<ViewModelLanguageDefinition> Languages { get; }

        public ViewModelLanguageDefinition SelectedLanguage
        {
            get => this.selectedLanguage;
            set
            {
                if (value == null)
                {
                    value = this.Languages.First(l => l.LanguageDefinition is Language_en_us);
                }

                if (this.selectedLanguage == value)
                {
                    return;
                }

                this.selectedLanguage = value;

                this.TextAccept = value?.LanguageDefinition.AcceptText;

                this.NotifyThisPropertyChanged();
            }
        }

        public string TextAccept { get; set; }

        protected override void DisposeViewModel()
        {
            ClientLanguagesManager.CurrentLanguageDefinitionChanged
                -= this.CurrentLanguageDefinitionChangedHandler;
        }

        private void CurrentLanguageDefinitionChangedHandler()
        {
            this.RefreshSelectedLanguage();
        }

        private void ExecuteCommandAccept()
        {
            ClientLanguagesManager.CurrentLanguageDefinition = this.selectedLanguage?.LanguageDefinition;
            MenuLanguageSelection.IsDisplayed = false;
        }

        private void RefreshSelectedLanguage()
        {
            var language = Api.Shared.IsRequiresLanguageSelection
                               ? ClientLanguagesManager.GetLanguage(Api.Shared.SystemLanguageTag)
                               : ClientLanguagesManager.CurrentLanguageDefinition;

            this.SelectedLanguage = this.Languages.FirstOrDefault(l => l.LanguageDefinition == language);
        }
    }
}