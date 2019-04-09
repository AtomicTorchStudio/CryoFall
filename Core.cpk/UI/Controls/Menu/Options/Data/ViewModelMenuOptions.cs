namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientOptions;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuOptions : BaseViewModel
    {
        public const string DialogResetOptions =
            "Do you want to reset the [b]{0}[/b] options?[br]Default values will be applied and saved.";

        private readonly IReadOnlyList<ProtoOptionsCategory> options;

        private DialogWindow dialogApplySettings;

        private TabItem selectedTab;

        public ViewModelMenuOptions(IReadOnlyList<ProtoOptionsCategory> options)
        {
            this.options = options;

            this.CommandApply = new ActionCommand(this.ExecuteCommandApply);
            this.CommandReset = new ActionCommand(this.ExecuteCommandReset);
            this.CommandCancel = new ActionCommand(this.ExecuteCommandCancel);

            foreach (var optionsCategory in options)
            {
                optionsCategory.Modified += this.OptionsCategoryModifiedHandler;
            }
        }

        public ViewModelMenuOptions()
        {
        }

        public bool AreApplyAndCancelButtonsEnabled { get; private set; }

        public BaseCommand CommandApply { get; }

        public BaseCommand CommandCancel { get; }

        public BaseCommand CommandReset { get; }

        public TabItem SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab == value)
                {
                    return;
                }

                if (this.selectedTab != null)
                {
                    var optionsCategory = (ProtoOptionsCategory)this.selectedTab.Tag;
                    if (optionsCategory.IsModified)
                    {
                        // cannot change tab instantly - the settings are modified but not applied
                        this.ShowWindowToApplySettings(
                            () => { this.SelectedTab = value; });
                        // force keeping current tab
                        this.NotifyThisPropertyChanged();
                        return;
                    }
                }

                //Api.Logger.WriteDev("Selected tab changed to: " + value?.Tag);
                this.selectedTab = value;
                this.UpdateButtonsStatus();

                this.NotifyThisPropertyChanged();
            }
        }

        private ProtoOptionsCategory SelectedOptionsCategory => (ProtoOptionsCategory)this.selectedTab.Tag;

        public bool CheckCanHide(Action callbackOnHide)
        {
            if (this.selectedTab == null)
            {
                return true;
            }

            var optionsCategory = (ProtoOptionsCategory)this.selectedTab.Tag;
            if (optionsCategory.IsModified)
            {
                this.ShowWindowToApplySettings(callbackOnHide);
                return false;
            }

            return true;
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            if (this.options == null)
            {
                return;
            }

            foreach (var optionsCategory in this.options)
            {
                optionsCategory.Modified -= this.OptionsCategoryModifiedHandler;
            }
        }

        private void ExecuteCommandApply()
        {
            var selectedOptionsCategory = this.SelectedOptionsCategory;
            selectedOptionsCategory.ApplyAndSave();
        }

        private void ExecuteCommandCancel()
        {
            var selectedOptionsCategory = this.SelectedOptionsCategory;
            selectedOptionsCategory.Cancel();
        }

        private void ExecuteCommandReset()
        {
            var selectedOptionsCategory = this.SelectedOptionsCategory;
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                string.Format(DialogResetOptions, selectedOptionsCategory.Name),
                okAction: () => selectedOptionsCategory.Reset(),
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.No,
                hideCancelButton: false,
                focusOnCancelButton: true);
        }

        private void OptionsCategoryModifiedHandler(ProtoOptionsCategory optionsCategory)
        {
            if (optionsCategory == this.selectedTab.Tag)
            {
                this.UpdateButtonsStatus();
            }
        }

        private void ShowWindowToApplySettings(Action onContinue)
        {
            if (this.dialogApplySettings != null)
            {
                WindowsManager.BringToFront(this.dialogApplySettings.Window);
                return;
            }

            this.dialogApplySettings = DialogWindow.ShowDialog(
                CoreStrings.MenuOptions_DialogUnappliedChanges_Title,
                CoreStrings.MenuOptions_DialogUnappliedChanges_Message,
                okText: CoreStrings.Button_Apply,
                cancelText: CoreStrings.Button_Cancel,
                okAction: () =>
                          {
                              this.dialogApplySettings = null;
                              this.ExecuteCommandApply();
                              onContinue();
                          },
                cancelAction: () =>
                              {
                                  this.dialogApplySettings = null;
                                  this.ExecuteCommandCancel();
                                  onContinue();
                              });
        }

        private void UpdateButtonsStatus()
        {
            this.AreApplyAndCancelButtonsEnabled = this.SelectedOptionsCategory.IsModified;
        }
    }
}