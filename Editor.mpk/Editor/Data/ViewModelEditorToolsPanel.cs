namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorToolsPanel : BaseViewModel
    {
        private ViewModelEditorTool selectedTool;

        public ViewModelEditorToolsPanel()
        {
            var toolsViewModels = new List<ViewModelEditorTool>();
            if (IsDesignTime)
            {
                toolsViewModels.Add(new ViewModelEditorTool(new EditorToolPointer()));
                toolsViewModels.Add(new ViewModelEditorTool(new EditorToolPointer()));
                toolsViewModels.Add(new ViewModelEditorTool(new EditorToolPointer()));
                toolsViewModels.Add(new ViewModelEditorTool(new EditorToolPointer()));
            }
            else
            {
                foreach (var tool in Api.FindProtoEntities<BaseEditorTool>()
                                        .OrderBy(t => t.Order))
                {
                    var editorToolViewModel = new ViewModelEditorTool(tool);
                    editorToolViewModel.OnIsSelectedChanged = this.ToolIsSelectedChanged;
                    toolsViewModels.Add(editorToolViewModel);
                }
            }

            this.ToolsCollection = toolsViewModels;
            this.SelectedTool = toolsViewModels[0];
        }

        public ViewModelEditorTool SelectedTool
        {
            get => this.selectedTool;
            set
            {
                if (this.selectedTool == value)
                {
                    return;
                }

                this.selectedTool = value;

                if (this.selectedTool is not null)
                {
                    this.selectedTool.IsSelected = true;
                }

                foreach (var tool in this.ToolsCollection)
                {
                    if (tool != this.selectedTool)
                    {
                        tool.IsSelected = false;
                    }
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public IReadOnlyList<ViewModelEditorTool> ToolsCollection { get; }

        private void ToolIsSelectedChanged(ViewModelEditorTool tool)
        {
            if (tool.IsSelected)
            {
                this.SelectedTool = tool;
            }
        }
    }
}