namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelHUDMiniMap : BaseViewModel
    {
        private static readonly IReadOnlyList<Vector2Ushort> PresetsSize
            = new List<Vector2Ushort>()
            {
                (0, 0),
                (146, 110),
                (170, 128),
                (230, 173)
            };

        private static readonly IReadOnlyList<double> PresetsZoom
            = new List<double>()
            {
                0.25,
                0.33,
                0.4,
                0.5
            };

        private static readonly IClientStorage StorageSize;

        private static readonly IClientStorage StorageZoom;

        private readonly Action callbackSizeOrZoomChanged;

        [ViewModelNotAutoDisposeField]
        private readonly FrameworkElement rootElement;

        private double zoom;

        static ViewModelHUDMiniMap()
        {
            StorageSize = Api.Client.Storage.GetStorage("Gameplay/MiniMapSize");
            StorageSize.RegisterType(typeof(Vector2Ushort));

            StorageZoom = Api.Client.Storage.GetStorage("Gameplay/MiniMapZoom");
        }

        public ViewModelHUDMiniMap(FrameworkElement rootElement, Action callbackSizeOrZoomChanged)
        {
            this.rootElement = rootElement;
            this.callbackSizeOrZoomChanged = callbackSizeOrZoomChanged;
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.CommandChangeSize = new ActionCommandWithParameter(this.ExecuteCommandChangeSize);
            this.CommandChangeZoom = new ActionCommandWithParameter(this.ExecuteCommandChangeZoom);

            // apply size
            if (!StorageSize.TryLoad(out Vector2Ushort settingSize))
            {
                settingSize = PresetsSize[PresetsSize.Count - 1];
            }

            this.ApplySizePreset(FindSizePresetIndex(settingSize.X), save: false);

            // apply zoom
            if (!StorageZoom.TryLoad(out double settingZoom))
            {
                settingZoom = PresetsZoom[PresetsZoom.Count - 1];
            }

            this.ApplyZoomPreset(FindZoomPresetIndex(settingZoom));
        }

        public bool CanDecreaseSize { get; private set; }

        public bool CanIncreaseSize { get; private set; }

        public bool CanZoomIn { get; private set; }

        public bool CanZoomOut { get; private set; }

        public ActionCommandWithParameter CommandChangeSize { get; }

        public ActionCommandWithParameter CommandChangeZoom { get; }

        public ushort ControlHeight { get; private set; } = 100;

        public ushort ControlWidth { get; private set; } = 100;

        public bool IsMapVisible => this.ControlWidth > 0;

        public bool IsMouseOverIncludingHidden { get; set; }

        public double Zoom
        {
            get => this.zoom;
            set
            {
                if (this.zoom == value)
                {
                    return;
                }

                this.zoom = value;
                this.NotifyThisPropertyChanged();

                StorageZoom.Save(this.zoom);

                var presetIndex = FindZoomPresetIndex(this.Zoom);
                this.CanZoomOut = presetIndex > 0;
                this.CanZoomIn = presetIndex < PresetsZoom.Count - 1;

                this.callbackSizeOrZoomChanged?.Invoke();
            }
        }

        public double ZoomMax => PresetsZoom[PresetsZoom.Count - 1];

        public double ZoomMin => PresetsZoom[0];

        protected override void DisposeViewModel()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            base.DisposeViewModel();
        }

        private static int FindSizePresetIndex(ushort currentWidth)
        {
            var index = 0;
            for (; index < PresetsSize.Count; index++)
            {
                var preset = PresetsSize[index];
                if (preset.X >= currentWidth)
                {
                    // preset found
                    return index;
                }
            }

            return index - 1;
        }

        private static int FindZoomPresetIndex(double currentZoom)
        {
            var index = 0;
            for (; index < PresetsZoom.Count; index++)
            {
                var preset = PresetsZoom[index];
                if (preset >= currentZoom)
                {
                    // preset found
                    return index;
                }
            }

            return index - 1;
        }

        private void ApplySizePreset(int presetIndex, bool save)
        {
            var preset = PresetsSize[presetIndex];
            this.ControlWidth = preset.X;
            this.ControlHeight = preset.Y;
            this.NotifyPropertyChanged(nameof(this.IsMapVisible));

            if (save)
            {
                StorageSize.Save(preset);
            }

            this.CanDecreaseSize = presetIndex > 0;
            this.CanIncreaseSize = presetIndex < PresetsSize.Count - 1;
        }

        private void ApplyZoomPreset(int presetIndex)
        {
            var preset = PresetsZoom[presetIndex];
            this.Zoom = preset;
        }

        private void ExecuteCommandChangeSize(object obj)
        {
            var isIncrease = "+".Equals((string)obj, StringComparison.Ordinal);
            var index = FindSizePresetIndex(this.ControlWidth);

            if (isIncrease)
            {
                index++;
            }
            else
            {
                index--;
            }

            index = MathHelper.Clamp(index, 0, PresetsSize.Count - 1);
            this.ApplySizePreset(index, save: true);
            this.callbackSizeOrZoomChanged();
        }

        private void ExecuteCommandChangeZoom(object obj)
        {
            var isIncrease = "+".Equals((string)obj, StringComparison.Ordinal);
            var index = FindZoomPresetIndex(this.Zoom);

            if (isIncrease)
            {
                index++;
            }
            else
            {
                index--;
            }

            index = MathHelper.Clamp(index, 0, PresetsZoom.Count - 1);
            this.ApplyZoomPreset(index);
        }

        private void Update()
        {
            var mouseScreenPosition = Api.Client.Input.MouseScreenPosition;
            var point = new Point(mouseScreenPosition.X, mouseScreenPosition.Y);
            point = this.rootElement.PointFromScreen(point);
            var hitTest = VisualTreeHelper.HitTest(this.rootElement, point);

            this.IsMouseOverIncludingHidden = hitTest.VisualHit is not null;
        }
    }
}