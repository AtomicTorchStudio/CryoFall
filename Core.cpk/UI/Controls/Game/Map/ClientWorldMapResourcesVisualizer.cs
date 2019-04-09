namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientWorldMapResourcesVisualizer : IDisposable, IWorldMapVisualizer
    {
        private readonly List<(WorldMapResourceMark mark, FrameworkElement mapControl)> visualizedMarks
            = new List<(WorldMapResourceMark, FrameworkElement)>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapResourcesVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;

            WorldMapResourceMarksSystem.ClientMarkAdded += this.MarkAddedHandler;
            WorldMapResourceMarksSystem.ClientMarkRemoved += this.MarkRemovedHandler;

            foreach (var mark in WorldMapResourceMarksSystem.ClientEnumerateMarks())
            {
                this.MarkAddedHandler(mark);
            }
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            WorldMapResourceMarksSystem.ClientMarkAdded -= this.MarkAddedHandler;
            WorldMapResourceMarksSystem.ClientMarkRemoved -= this.MarkRemovedHandler;

            if (this.visualizedMarks.Count > 0)
            {
                foreach (var visualizedArea in this.visualizedMarks.ToList())
                {
                    this.MarkRemovedHandler(visualizedArea.mark);
                }
            }
        }

        private FrameworkElement GetMapControl(WorldMapResourceMark mark)
        {
            switch (mark.ProtoWorldObject)
            {
                case ObjectDepositOilSeep _:
                    return new WorldMapMarkResourceOil();

                case ObjectDepositGeothermalSpring _:
                    return new WorldMapMarkResourceLithium();

                default:
                    return null;
            }
        }

        private void MarkAddedHandler(WorldMapResourceMark mark)
        {
            var mapControl = this.GetMapControl(mark);
            if (mapControl == null)
            {
                Api.Logger.Warning("Unknown world object mark: "
                                   + mark.ProtoWorldObject
                                   + " - there is no UI control for this world object prototype");
                return;
            }

            var canvasPosition = this.worldMapController.WorldToCanvasPosition(mark.Position.ToVector2D()
                                                                               + mark.ProtoWorldObject.Layout.Center);
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 16);

            this.worldMapController.AddControl(mapControl);

            this.visualizedMarks.Add((mark, mapControl));
        }

        private void MarkRemovedHandler(WorldMapResourceMark mark)
        {
            for (var index = 0; index < this.visualizedMarks.Count; index++)
            {
                var entry = this.visualizedMarks[index];
                if (!entry.mark.Equals(mark))
                {
                    continue;
                }

                this.visualizedMarks.RemoveAt(index);
                this.worldMapController.RemoveControl(entry.mapControl);
            }
        }
    }
}