namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class EditorToolZonePointedZoneNamesControl : BaseUserControl
    {
        private readonly Dictionary<Color, Brush> cachedBrushes = new();

        private Vector2Ushort lastTilePosition;

        private List<IProtoZone> tempListProtoZones = new();

        private TextBlock textBlock;

        public void ForceRefresh()
        {
            this.lastTilePosition = default;
            this.Update();
        }

        protected override void InitControl()
        {
            this.textBlock = this.GetByName<TextBlock>("TextBlock");
        }

        protected override void OnLoaded()
        {
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.lastTilePosition = default;
        }

        private Brush GetCachedBrush(Color color)
        {
            if (!this.cachedBrushes.TryGetValue(color, out var brush))
            {
                this.cachedBrushes[color] = brush = new SolidColorBrush() { Color = color };
            }

            return brush;
        }

        private void Update()
        {
            var tilePosition = Api.Client.Input.MousePointedTilePosition;
            if (this.lastTilePosition == tilePosition)
            {
                return;
            }

            this.lastTilePosition = tilePosition;

            try
            {
                foreach (var provider in ClientZoneProvider.AllProviders)
                {
                    if (provider.IsFilledPosition(tilePosition))
                    {
                        this.tempListProtoZones.Add(provider.ProtoZone);
                    }
                }

                this.textBlock.Inlines.Clear();

                this.textBlock.Inlines.Add(new Run() { Text = "Zones under the mouse cursor:   " });

                if (this.tempListProtoZones.Count <= 0)
                {
                    this.textBlock.Inlines.Add(new Run() { Text = "<none>" });
                }
                else
                {
                    for (var index = 0; index < this.tempListProtoZones.Count; index++)
                    {
                        var protoZone = this.tempListProtoZones[index];
                        var isLast = index == this.tempListProtoZones.Count - 1;

                        this.textBlock.Inlines.Add(new Run()
                        {
                            Text = isLast
                                       ? protoZone.Name
                                       : protoZone.Name + ",   ",
                            Foreground = this.GetCachedBrush(EditorActiveToolZones.Instance?.GetForeground(protoZone)
                                                             ?? Colors.White)
                        });
                    }
                }
            }
            finally
            {
                this.tempListProtoZones.Clear();
            }
        }
    }
}