namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelCharacterStyleSetting : BaseViewModel
    {
        private readonly Action valueChangedCallback;

        private int maxValue;

        private int value;

        public ViewModelCharacterStyleSetting(string title, int maxValue, Action valueChangedCallback = null)
        {
            this.Title = title;
            this.MaxValue = maxValue;
            this.valueChangedCallback = valueChangedCallback;

            this.CommandSelectNext = new ActionCommand(() => this.Value++);
            this.CommandSelectPrevious = new ActionCommand(() => this.Value--);
        }

        public BaseCommand CommandSelectNext { get; }

        public BaseCommand CommandSelectPrevious { get; }

        public int MaxValue
        {
            get => this.maxValue;
            set
            {
                if (this.maxValue == value)
                {
                    return;
                }

                this.maxValue = value;
                this.NotifyThisPropertyChanged();

                // refresh current value
                this.Value = this.value;
            }
        }

        public string Title { get; }

        public int Value
        {
            get => this.value;
            set
            {
                if (value < 0)
                {
                    value = this.MaxValue;
                }
                else if (value > this.MaxValue)
                {
                    value = 0;
                }

                if (this.value == value)
                {
                    return;
                }

                this.value = value;
                this.NotifyThisPropertyChanged();

                this.valueChangedCallback();
            }
        }
    }
}