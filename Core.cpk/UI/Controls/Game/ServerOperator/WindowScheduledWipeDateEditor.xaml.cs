namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.ServerOperator
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowScheduledWipeDateEditor : BaseUserControlWithWindow
    {
        public Action<DateTime?> SaveAction;

        private Button buttonCancel;

        private Button buttonSave;

        private DateTimeSelectionControl dateTimeSelectionControl;

        private RadioButton radioButtonWipeDateNotSpecified;

        private RadioButton radioButtonWipeDateSelected;

        private DateTime? selectedDateUtc;

        public void SetSelectedDateUtc(DateTime? dateTime)
        {
            this.selectedDateUtc = dateTime;
        }

        protected override void InitControlWithWindow()
        {
            this.buttonSave = this.GetByName<Button>("ButtonSave");
            this.buttonCancel = this.GetByName<Button>("ButtonCancel");
            this.radioButtonWipeDateNotSpecified = this.GetByName<RadioButton>("RadioButtonWipeDateNotSpecified");
            this.radioButtonWipeDateSelected = this.GetByName<RadioButton>("RadioButtonWipeDateSelected");
            this.dateTimeSelectionControl = this.GetByName<DateTimeSelectionControl>("DateTimeSelectionControl");
        }

        protected override void OnLoaded()
        {
            this.dateTimeSelectionControl.SelectedDate = this.selectedDateUtc
                                                         ?? DateTime.Today
                                                                    .ToUniversalTime()
                                                                    .Date
                                                                    .AddDays(1)
                                                                    .AddHours(13);

            this.radioButtonWipeDateSelected.IsChecked = this.selectedDateUtc.HasValue;
            this.radioButtonWipeDateNotSpecified.IsChecked = !this.selectedDateUtc.HasValue;

            this.buttonCancel.Click += this.ButtonCancelClickHandler;
            this.buttonSave.Click += this.ButtonSaveClickHandler;
            this.dateTimeSelectionControl.SelectedDateChanged += this.SelectedDateChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.buttonCancel.Click -= this.ButtonCancelClickHandler;
            this.buttonSave.Click -= this.ButtonSaveClickHandler;
            this.dateTimeSelectionControl.SelectedDateChanged -= this.SelectedDateChangedHandler;
        }

        private void ButtonCancelClickHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Window.Close(DialogResult.Cancel);
        }

        private void ButtonSaveClickHandler(object sender, RoutedEventArgs arg1)
        {
            var dateTime = this.selectedDateUtc
                               = this.radioButtonWipeDateSelected.IsChecked ?? false
                                     ? (DateTime?)this.dateTimeSelectionControl.SelectedDate
                                     : null;

            if (dateTime.HasValue
                && dateTime.Value.ToLocalTime() <= DateTime.Now)
            {
                DialogWindow.ShowMessage(CoreStrings.TitleAttention,
                                         CoreStrings.CannotSelectDateInThePast,
                                         closeByEscapeKey: true,
                                         zIndexOffset: this.Window.ZIndexOffset);
                return;
            }

            this.Window.Close(DialogResult.OK);
            this.SaveAction?.Invoke(dateTime);
        }

        private void SelectedDateChangedHandler()
        {
            // any date is selected - ensure the checkbox is auto checked
            this.radioButtonWipeDateSelected.IsChecked = true;
        }
    }
}