namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelLanguageDefinition : BaseViewModel
    {
        public readonly ProtoLanguageDefinition LanguageDefinition;

        public ViewModelLanguageDefinition(ProtoLanguageDefinition languageDefinition)
        {
            this.LanguageDefinition = languageDefinition;
        }

        public string Description => this.LanguageDefinition.Description;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.LanguageDefinition.Icon);

        public string NameEnglish => this.LanguageDefinition.NameEnglish;

        public string NameNative => this.LanguageDefinition.NameNative;
    }
}