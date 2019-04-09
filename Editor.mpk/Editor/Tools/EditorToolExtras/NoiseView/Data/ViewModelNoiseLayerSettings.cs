namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelNoiseLayerSettings : BaseViewModel
    {
        private readonly Action callbackRefresh;

        private Color color;

        private double maxValue = double.MaxValue;

        public ViewModelNoiseLayerSettings(
            double maxValue,
            Color color,
            BaseCommand commandDelete,
            Action callbackRefresh)
        {
            this.MaxValue = maxValue;
            this.Color = color;

            this.CommandDelete = commandDelete;
            this.callbackRefresh = callbackRefresh;
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
                this.Refresh();
            }
        }

        public BaseCommand CommandDelete { get; }

        public BaseCommand CommandPickColor => new ActionCommandWithParameter(ExecuteCommandPickColor);

        public double MaxValue
        {
            get => this.maxValue;
            set
            {
                value = MathHelper.Clamp(value, 0, 1);
                if (this.maxValue == value)
                {
                    return;
                }

                this.maxValue = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        private static void ExecuteCommandPickColor(object obj)
        {
            var layer = (ViewModelNoiseLayerSettings)obj;
            var window = new WindowColorPicker(layer.Color);
            window.ColorSelected = () => layer.Color = window.ViewModel.Color;
            Api.Client.UI.LayoutRootChildren.Add(window);
        }

        private void Refresh()
        {
            this.callbackRefresh?.Invoke();
        }
    }
}