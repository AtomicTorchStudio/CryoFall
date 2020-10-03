namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DateTimeSelectionControl : BaseUserControl
    {
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(nameof(SelectedDate),
                                        typeof(DateTime),
                                        typeof(DateTimeSelectionControl),
                                        new PropertyMetadata(default(DateTime), SelectedDatePropertyChanged));

        private ViewModelDateTimeSelectionControl viewModel;

        public event Action SelectedDateChanged;

        public DateTime SelectedDate
        {
            get => (DateTime)this.GetValue(SelectedDateProperty);
            set => this.SetValue(SelectedDateProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelDateTimeSelectionControl()
            {
                SelectedDate = this.SelectedDate
            };

            this.SetBinding(SelectedDateProperty,
                            // bind to the foreground of the hidden textblock
                            new Binding(nameof(ViewModelDateTimeSelectionControl.SelectedDate))
                                { Source = this.viewModel });
        }

        protected override void OnUnloaded()
        {
            var vm = this.viewModel;
            var selectedDate = vm.SelectedDate;

            BindingOperations.ClearBinding(this, SelectedDateProperty);
            this.DataContext = null;
            this.viewModel = null;
            vm.Dispose();

            this.SelectedDate = selectedDate;
        }

        private static void SelectedDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DateTimeSelectionControl)d;
            if (!control.isLoaded
                || control.viewModel is null)
            {
                return;
            }

            control.viewModel.SelectedDate = control.SelectedDate;
            control.SelectedDateChanged?.Invoke(); // assume the change was done from the UI by user
        }
    }
}