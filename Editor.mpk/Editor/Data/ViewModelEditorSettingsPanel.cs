namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Physics;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorSettingsPanel : BaseViewModel
    {
        private static ViewModelEditorSettingsPanel instance;

        public ViewModelEditorSettingsPanel()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (instance != null)
            {
                throw new Exception("Instance already created");
            }

            ClientComponentDebugGrid.IsDrawingChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsGridEnabled));

            ClientDebugGuidesManager.IsDrawingChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsGuidesEnabled));

            ClientTileBlendHelper.IsBlendingEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsTerrainBlendingEnabled));

            ClientTileDecalHelper.IsDecalsEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsTerrainDecalsEnabled));

            ClientComponentPhysicsSpaceVisualizer.IsInstanceExistChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsPhysicsSpaceVisualizerEnabled));

            ClientComponentUpdateHelper.UpdateCallback += this.Update;
        }

        public static ViewModelEditorSettingsPanel Instance
            => instance ?? (instance = new ViewModelEditorSettingsPanel());

        public ushort CursorWorldPositionX { get; set; }

        public ushort CursorWorldPositionY { get; set; }

        public bool IsGridEnabled
        {
            get => ClientComponentDebugGrid.Instance.IsDrawing;
            set => ClientComponentDebugGrid.Instance.IsDrawing = value;
        }

        public bool IsGuidesEnabled
        {
            get => ClientDebugGuidesManager.Instance.IsGuidesEnabled;
            set => ClientDebugGuidesManager.Instance.IsGuidesEnabled = value;
        }

        public bool IsPhysicsSpaceVisualizerEnabled
        {
            get => ClientComponentPhysicsSpaceVisualizer.IsVisualizerEnabled;
            set => ClientComponentPhysicsSpaceVisualizer.IsVisualizerEnabled = value;
        }

        public bool IsTerrainBlendingEnabled
        {
            get => ClientTileBlendHelper.IsBlendingEnabled;
            set => ClientTileBlendHelper.IsBlendingEnabled = value;
        }

        public bool IsTerrainDecalsEnabled
        {
            get => ClientTileDecalHelper.IsDecalsEnabled;
            set => ClientTileDecalHelper.IsDecalsEnabled = value;
        }

        private void Update()
        {
            var position = Api.Client.Input.MouseWorldPosition.ToVector2Ushort();
            position -= Api.Client.World.WorldBounds.Offset;
            this.CursorWorldPositionX = position.X;
            this.CursorWorldPositionY = position.Y;
        }
    }
}