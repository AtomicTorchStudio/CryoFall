namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelNoisePresetSettings : BaseViewModel
    {
        private readonly Action callbackRefresh;

        private Color color;

        private bool isDebug;

        private bool isEnabled = true;

        private double maxValue = 1;

        private double minValue = 0.75;

        public ViewModelNoisePresetSettings(
            ViewModelNoiseSettings noiseSetttings,
            Color color,
            BaseCommand commandClone,
            Action callbackRefresh,
            BaseCommand commandDelete)
        {
            this.NoiseSetttings = noiseSetttings;
            this.color = color;
            this.CommandClone = commandClone;
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

        public BaseCommand CommandClone { get; }

        public BaseCommand CommandCollapse => new ActionCommand(this.ExecuteCommandCollapse);

        public BaseCommand CommandCopyCode => new ActionCommand(this.ExecuteCommandCopyCode);

        public BaseCommand CommandDelete { get; }

        public BaseCommand CommandPasteCode => new ActionCommand(this.ExecuteCommandPasteCode);

        public BaseCommand CommandPickColor => new ActionCommandWithParameter(ExecuteCommandPickColor);

        public bool IsCollapsed { get; set; }

        public bool IsDebug
        {
            get => this.isDebug;
            set
            {
                if (this.isDebug == value)
                {
                    return;
                }

                this.isDebug = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

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

        public double MinValue
        {
            get => this.minValue;
            set
            {
                value = MathHelper.Clamp(value, 0, 1);
                if (this.minValue == value)
                {
                    return;
                }

                this.minValue = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public ViewModelNoiseSettings NoiseSetttings { get; private set; }

        public ViewModelNoisePresetSettings Clone()
        {
            var result = (ViewModelNoisePresetSettings)this.MemberwiseClone();
            result.NoiseSetttings = this.NoiseSetttings.Clone();
            return result;
        }

        public string GetNoiseSelectorCode()
        {
            return new StringBuilder("new NoiseSelector(" + Environment.NewLine)
                   .Append("from: ")
                   .Append(this.minValue.ToString(CultureInfo.InvariantCulture))
                   .AppendLine(",")
                   .Append("to: ")
                   .Append(this.maxValue.ToString(CultureInfo.InvariantCulture))
                   .AppendLine(",")
                   .Append("noise: ")
                   .Append(this.NoiseSetttings.GetNoiseCode())
                   .Append(")")
                   .ToString();
        }

        public bool IsNoiseValueInRange(double noiseValue)
        {
            return noiseValue >= this.MinValue
                   && noiseValue <= this.MaxValue;
        }

        private static void ExecuteCommandPickColor(object obj)
        {
            var noiseLayer = (ViewModelNoisePresetSettings)obj;
            var window = new WindowColorPicker(noiseLayer.Color);
            window.ColorSelected = () => noiseLayer.Color = window.ViewModel.Color;
            Api.Client.UI.LayoutRootChildren.Add(window);
        }

        private void ExecuteCommandCollapse()
        {
            this.IsCollapsed = !this.IsCollapsed;
        }

        private void ExecuteCommandCopyCode()
        {
            Api.Client.Core.CopyToClipboard(this.GetNoiseSelectorCode());
        }

        private void ExecuteCommandPasteCode()
        {
            this.NoiseSetttings.CommandPasteCode.Execute(null);

            var text = Api.Client.Core.GetClipboard();
            var indexOfRangeDouble = text.IndexOf("RangeDouble(", StringComparison.Ordinal);
            int firstArgStart,
                firstArgEnd,
                secondArgStart,
                secondArgEnd;

            if (indexOfRangeDouble >= 0)
            {
                firstArgStart = indexOfRangeDouble + "RangeDouble(".Length;
                firstArgEnd = text.IndexOf(',', firstArgStart);
                secondArgStart = firstArgEnd + 1;
                secondArgEnd = text.IndexOf(')', firstArgStart);
            }
            else
            {
                // no range definition
                // find "from" and "to" definitions
                var indexOfFrom = text.IndexOf("from:", StringComparison.Ordinal);
                firstArgStart = indexOfFrom + "from:".Length;
                firstArgEnd = text.IndexOf(',', firstArgStart);
                secondArgStart = text.IndexOf("to:", firstArgEnd, StringComparison.Ordinal) + "to:".Length;
                secondArgEnd = text.IndexOf(',', secondArgStart);
            }

            this.MinValue = Parse(firstArgStart,  firstArgEnd);
            this.MaxValue = Parse(secondArgStart, secondArgEnd);

            double Parse(int start, int end)
            {
                var v = text.Substring(start, end - start).Trim(' ');
                return double.Parse(v, CultureInfo.InvariantCulture);
            }
        }

        private void Refresh()
        {
            this.callbackRefresh?.Invoke();
        }
    }
}