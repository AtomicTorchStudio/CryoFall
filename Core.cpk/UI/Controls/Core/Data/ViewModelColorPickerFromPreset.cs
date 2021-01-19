namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    public class ViewModelColorPickerFromPreset : BaseViewModel
    {
        private readonly Action valueChangedCallback;

        private Color color;

        public ViewModelColorPickerFromPreset(
            string title,
            IReadOnlyCollection<Color> availableColors,
            Action valueChangedCallback = null)
        {
            this.valueChangedCallback = valueChangedCallback;
            this.Title = title;
            this.AvailableColors = availableColors;
        }

        public IReadOnlyCollection<Color> AvailableColors { get; }

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