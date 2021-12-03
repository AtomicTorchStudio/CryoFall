namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.DebugTools
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelDebugToolsOverlay : BaseViewModel
    {
        private static ViewModelDebugToolsOverlay instance;

        private ViewModelDebugToolsOverlay()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (instance is not null)
            {
                throw new Exception("Instance already created");
            }

            ClientComponentDebugGrid.IsDrawingChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsGridEnabled));

            ClientDebugGuidesManager.IsDrawingChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsGuidesEnabled));

            ClientPostEffectsManager.IsPostEffectsEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsPostEffectsEnabled));

            ClientTileBlendHelper.IsBlendingEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsTerrainBlendingEnabled));

            ClientTileDecalHelper.IsDecalsEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsTerrainDecalsEnabled));

            ClientComponentPhysicsSpaceVisualizer.IsInstanceExistChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsPhysicsSpaceVisualizerEnabled));

            ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabledChanged
                += () => this.NotifyPropertyChanged(nameof(this.IsClientLagPredictionEnabled));
        }

        public static ViewModelDebugToolsOverlay Instance
            => instance ??= new ViewModelDebugToolsOverlay();

        public bool IsClientLagPredictionEnabled
        {
            get => ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled;
            set => ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled = value;
        }

        public bool IsClientLagPredictionVisualizerEnabled
        {
            get => ComponentLagPredictionVisualizer.IsVisualizerEnabled;
            set => ComponentLagPredictionVisualizer.IsVisualizerEnabled = value;
        }

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

        public bool IsPostEffectsEnabled
        {
            get => ClientPostEffectsManager.IsPostEffectsEnabled;
            set
            {
                if (!this.IsServerOperator)
                {
                    value = true;
                }

                ClientPostEffectsManager.IsPostEffectsEnabled = value;
            }
        }

        public bool IsServerOperator
            => Api.IsEditor
               || ServerOperatorSystem.ClientIsOperator();

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
    }
}