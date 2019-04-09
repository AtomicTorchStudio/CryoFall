namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;

    public class EditorToolGenerator : BaseEditorTool<EditorToolGeneratorItem>
    {
        private ViewModelEditorToolGeneratorSettings settings;

        public override string Name => "Generator tool";

        public override int Order => 60;

        public override BaseEditorActiveTool Activate(EditorToolGeneratorItem item)
        {
            var activeTool = new EditorToolAreaSelectorActive();
            this.settings.ViewModelLocationSettings = activeTool.LocationSettingsViewModel;
            this.settings.SelectedGenerator = item.Generator;
            return activeTool;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var settingsControl = new EditorToolGeneratorSettings();
            if (this.settings == null)
            {
                this.settings = new ViewModelEditorToolGeneratorSettings();
            }

            settingsControl.DataContext = this.settings;
            return settingsControl;
        }

        protected override void SetupFilters(List<EditorToolItemFilter<EditorToolGeneratorItem>> filters)
        {
            // no filters
        }

        protected override void SetupItems(List<EditorToolGeneratorItem> items)
        {
            items.AddRange(
                FindProtoEntities<BaseEditorToolGeneratorAlgorithm>()
                    .Select(g => new EditorToolGeneratorItem(g)));
        }
    }
}