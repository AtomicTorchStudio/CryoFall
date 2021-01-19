namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class GeneralOptionHideQuestsPanel : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public const string Dialog_Text =
            @"Quests are a vital part of the game, especially if you are playing for the first time where they serve as a guide and a tutorial for new players.
              [br]
              [br]Are you certain you want to disable the quests panel?";

        public static event Action IsQuestsPanelHiddenChanged;

        public static bool IsQuestsPanelHidden { get; private set; }

        public override bool Default => false;

        public override string Name => "Hide quests panel";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionDisplayHealthbarAboveCurrentCharacter>();

        public override bool ValueProvider
        {
            get => IsQuestsPanelHidden;
            set
            {
                if (IsQuestsPanelHidden == value)
                {
                    return;
                }

                IsQuestsPanelHidden = value;
                Api.SafeInvoke(() => IsQuestsPanelHiddenChanged?.Invoke());
            }
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            var isQuestsPanelHidden = this.CurrentValue;
            if (fromUi && isQuestsPanelHidden)
            {
                // Tell user it's dangerous!
                DialogWindow.ShowDialog(
                    title: this.Name,
                    text: Dialog_Text,
                    textAlignment: TextAlignment.Left,
                    okText: CoreStrings.Yes,
                    cancelText: CoreStrings.Button_Cancel,
                    okAction: () => { },
                    cancelAction: () => { this.CurrentValue = false; },
                    autoWidth: false,
                    focusOnCancelButton: true);
            }
        }
    }
}