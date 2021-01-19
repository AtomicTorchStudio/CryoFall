namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelNoiseLayersSettings : BaseViewModel
    {
        private readonly Action callbackRefresh;

        private readonly BaseCommand commandDeleteLayer;

        private bool isLayersEnabled;

        public ViewModelNoiseLayersSettings(Action callbackRefresh)
        {
            this.callbackRefresh = callbackRefresh;
            this.CommandAddLayer = new ActionCommandWithParameter(this.ExecuteCommandAddLayer);
            this.commandDeleteLayer = new ActionCommandWithParameter(this.ExecuteCommandDeleteLayer);

            this.Layers = new ObservableCollection<ViewModelNoiseLayerSettings>()
            {
                new(0.25,
                    Color.FromRgb(0x00, 0x00, 0xFF),
                    this.commandDeleteLayer,
                    this.callbackRefresh),
                new(0.5,
                    Color.FromRgb(0x00, 0x88, 0x00),
                    this.commandDeleteLayer,
                    this.callbackRefresh),
                new(0.75,
                    Color.FromRgb(0x99, 0x55, 0x00),
                    this.commandDeleteLayer,
                    this.callbackRefresh),
                new(1,
                    Color.FromRgb(0xDD, 0xDD, 0xDD),
                    this.commandDeleteLayer,
                    this.callbackRefresh)
            };
        }

        public BaseCommand CommandAddLayer { get; }

        public bool IsLayersEnabled
        {
            get => this.isLayersEnabled;
            set
            {
                if (this.isLayersEnabled == value)
                {
                    return;
                }

                this.isLayersEnabled = value;
                this.NotifyThisPropertyChanged();
                this.callbackRefresh();
            }
        }

        public ObservableCollection<ViewModelNoiseLayerSettings> Layers { get; }

        public Color GetColor(double value)
        {
            var layersCount = this.Layers.Count;
            for (byte index = 0; index < layersCount; index++)
            {
                var layer = this.Layers[index];
                if (value <= layer.MaxValue)
                {
                    return layer.Color;
                }
            }

            return this.Layers[layersCount - 1].Color;
        }

        private void ExecuteCommandAddLayer(object obj)
        {
            var isAddToEnd = string.Equals(obj as string, "end");
            if (isAddToEnd && this.Layers.Count > 0)
            {
                var maxValue = this.Layers.Max(s => s.MaxValue) + 0.01;
                this.Layers.Add(new ViewModelNoiseLayerSettings(
                                    maxValue,
                                    Colors.White,
                                    this.commandDeleteLayer,
                                    this.callbackRefresh));
            }
            else
            {
                this.Layers.Insert(0,
                                   new ViewModelNoiseLayerSettings(
                                       0,
                                       Colors.White,
                                       this.commandDeleteLayer,
                                       this.callbackRefresh));
            }

            this.callbackRefresh();
        }

        private void ExecuteCommandDeleteLayer(object obj)
        {
            this.Layers.Remove((ViewModelNoiseLayerSettings)obj);
            this.callbackRefresh();
        }
    }
}