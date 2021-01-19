namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BaseWorldMapVisualizer : IDisposable
    {
        private readonly List<UIElement> controls = new();

        private readonly WorldMapController worldMapController;

        private bool isEnabled;

        protected BaseWorldMapVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;
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
                //Api.Logger.Dev($"Map visualizer go to state: IsEnabled={this.isEnabled} - {this.GetType().Name}");

                var visibility = this.isEnabled
                                     ? Visibility.Visible
                                     : Visibility.Collapsed;

                foreach (var control in this.controls)
                {
                    control.Visibility = visibility;
                }

                if (this.isEnabled)
                {
                    this.OnEnable();
                }
                else
                {
                    this.OnDisable();
                }
            }
        }

        public void Dispose()
        {
            this.IsEnabled = false;
            this.DisposeInternal();
        }

        protected void AddControl(UIElement control, bool scaleWithZoom = true)
        {
            if (!this.isEnabled)
            {
                control.Visibility = Visibility.Collapsed;
            }

            if (this.controls.Contains(control))
            {
                Api.Logger.Error("The control is already added: " + control + " in " + this);
                return;
            }

            this.controls.Add(control);
            this.worldMapController.AddControl(control, scaleWithZoom);
        }

        protected abstract void DisposeInternal();

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected void RemoveControl(UIElement control)
        {
            if (this.controls.Remove(control))
            {
                this.worldMapController.RemoveControl(control);
            }
        }

        protected Vector2D WorldToCanvasPosition(Vector2D worldPosition)
        {
            return this.worldMapController.WorldToCanvasPosition(worldPosition);
        }
    }
}