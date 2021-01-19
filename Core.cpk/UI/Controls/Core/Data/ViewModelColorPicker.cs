namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Windows.Media;

    public class ViewModelColorPicker : BaseViewModel
    {
        private readonly Action valueChangedCallback;

        private Color color;

        public ViewModelColorPicker(string title, Action valueChangedCallback = null)
        {
            this.valueChangedCallback = valueChangedCallback;
            this.Title = title;
        }

        public Color Color
        {
            get => this.color;
            set
            {
                if (this.color == value)
                {
                    return;
                }

                this.color = value;
                this.NotifyThisPropertyChanged();
                this.valueChangedCallback?.Invoke();
            }
        }

        public string Title { get; }
    }
}