namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelEditorToolZonesSettings : BaseViewModelBrush
    {
        private bool isFillZoneMode;

        private ViewModelProtoZone selectedZoneForBrush;

        public ViewModelEditorToolZonesSettings()
            : base(defaultBrushSize: 10,
                   maxBrushSize: 20)
        {
            if (IsDesignTime)
            {
                this.Zones = new List<ViewModelProtoZone>();

                return;
            }

            this.Zones = Api.FindProtoEntities<IProtoZone>()
                            .OrderBy(z => z.Name)
                            .Select(this.Wrap)
                            .ToList();

            this.CommandDeleteSelectedZoneObjects = new ActionCommand(this.ExecuteCommandDeleteSelectedZoneObjects);
            this.CommandClearSelectedZone = new ActionCommand(this.ExecuteCommandClearSelectedZone);
        }

        public event Action SelectedZoneForBrushChanged;

        public event Action SelectedZonesForRenderingChanged;

        public BaseCommand CommandClearSelectedZone { get; }

        public BaseCommand CommandDeleteSelectedZoneObjects { get; }

        public BaseCommand CommandInvokeSelectedZoneScriptsAsInitial =>
            new ActionCommand(this.ExecuteCommandInvokeSelectedZoneScriptsAsInitial);

        public BaseCommand CommandInvokeSelectedZoneScriptsAsTimer =>
            new ActionCommand(this.ExecuteCommandInvokeSelectedZoneScriptsAsTimer);

        public bool IsAllowZoneChangeOnlyOnTheSameHeight { get; set; }

        public bool IsAllowZoneChangeOnlyOnTheSameTileProto { get; set; }

        public bool IsFillZoneMode
        {
            get => this.isFillZoneMode;
            set
            {
                if (this.isFillZoneMode == value)
                {
                    return;
                }

                this.isFillZoneMode = value;
                this.NotifyThisPropertyChanged();

                if (this.isFillZoneMode)
                {
                    this.SelectedBrushSize = 1;
                    this.SelectedBrushShape = this.BrushShapes[0];
                }
            }
        }

        public ViewModelProtoZone SelectedZoneForBrush
        {
            get => this.selectedZoneForBrush;
            set
            {
                if (this.selectedZoneForBrush == value)
                {
                    return;
                }

                this.selectedZoneForBrush = value;
                this.NotifyThisPropertyChanged();
                if (value != null)
                {
                    value.IsRendered = true;
                }

                this.SelectedZoneForBrushChanged?.Invoke();
            }
        }

        public IReadOnlyList<ViewModelProtoZone> Zones { get; }

        private void ExecuteCommandClearSelectedZone()
        {
            if (this.selectedZoneForBrush == null)
            {
                return;
            }

            DialogWindow.ShowDialog(
                "Are you sure?",
                "Do you want to clear the zone completely?"
                + " (the zone entry will remain but it will be clean, you can undo this action if needed)",
                okAction: () => EditorZoneSystem.Instance.ClientClearSelectedZone(this.selectedZoneForBrush.Zone),
                hideCancelButton: false);
        }

        private void ExecuteCommandDeleteSelectedZoneObjects()
        {
            if (this.selectedZoneForBrush == null)
            {
                return;
            }

            EditorZoneSystem.Instance.ClientDeleteSelectedZoneObjects(this.selectedZoneForBrush.Zone);
        }

        private void ExecuteCommandInvokeSelectedZoneScriptsAsInitial()
        {
            if (this.selectedZoneForBrush != null)
            {
                EditorZoneSystem.Instance.ClientInvokeZoneScripts(this.selectedZoneForBrush.Zone, isInitialSpawn: true);
            }
        }

        private void ExecuteCommandInvokeSelectedZoneScriptsAsTimer()
        {
            if (this.selectedZoneForBrush != null)
            {
                EditorZoneSystem.Instance.ClientInvokeZoneScripts(this.selectedZoneForBrush.Zone,
                                                                  isInitialSpawn: false);
            }
        }

        private ViewModelProtoZone Wrap(IProtoZone z)
        {
            var viewModel = new ViewModelProtoZone(z);
            viewModel.IsRenderedChanged += this.ZoneIsRenderedChangedHandler;
            return viewModel;
        }

        private void ZoneIsRenderedChangedHandler(ViewModelProtoZone viewModelProtoZone)
        {
            if (viewModelProtoZone.IsRendered)
            {
                this.SelectedZoneForBrush = viewModelProtoZone;
            }

            this.SelectedZonesForRenderingChanged?.Invoke();
        }
    }
}