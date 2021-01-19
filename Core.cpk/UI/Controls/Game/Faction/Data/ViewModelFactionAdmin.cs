namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelFactionAdmin : BaseViewModel
    {
        private readonly FactionPrivateState factionPrivateState;

        public ViewModelFactionAdmin(FactionPrivateState factionPrivateState)
        {
            this.CommandEditFactionDescriptionPrivate = new ActionCommand(
                () => this.ExecuteCommandEditFactionDescription(isPrivateDescription: true));

            this.CommandEditFactionDescriptionPublic = new ActionCommand(
                () => this.ExecuteCommandEditFactionDescription(isPrivateDescription: false));

            this.factionPrivateState = factionPrivateState;
            this.factionPrivateState.ClientSubscribe(
                _ => _.DescriptionPrivate,
                () => this.NotifyPropertyChanged(nameof(this.DescriptionPrivate)),
                subscriptionOwner: this);

            this.factionPrivateState.ClientSubscribe(
                _ => _.DescriptionPublic,
                () => this.NotifyPropertyChanged(nameof(this.DescriptionPublic)),
                subscriptionOwner: this);
        }

        public BaseCommand CommandEditFactionDescriptionPrivate { get; }

        public BaseCommand CommandEditFactionDescriptionPublic { get; }

        public string DescriptionPrivate
        {
            get
            {
                var text = this.factionPrivateState?.DescriptionPrivate;
                return string.IsNullOrEmpty(text) ? CoreStrings.EmptyText : text;
            }
        }

        public string DescriptionPublic
        {
            get
            {
                var text = this.factionPrivateState?.DescriptionPublic;
                return string.IsNullOrEmpty(text) ? CoreStrings.EmptyText : text;
            }
        }

        private void ExecuteCommandEditFactionDescription(bool isPrivateDescription)
        {
            BbTextEditorHelper.ClientOpenTextEditor(
                originalText: isPrivateDescription
                                  ? this.factionPrivateState.DescriptionPrivate
                                  : this.factionPrivateState.DescriptionPublic,
                maxLength: isPrivateDescription
                               ? FactionSystem.MaxDescriptionLengthPrivate
                               : FactionSystem.MaxDescriptionLengthPublic,
                windowHeight: isPrivateDescription
                                  ? 450
                                  : 200,
                onSave: text => FactionSystem.ClientOfficerSetFactionDescription(text, isPrivateDescription));
        }
    }
}