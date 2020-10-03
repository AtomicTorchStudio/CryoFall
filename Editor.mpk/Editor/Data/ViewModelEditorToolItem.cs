namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorToolItem : BaseViewModel
    {
        public readonly BaseEditorToolItem ToolItem;

        private bool isSelected;

        public ViewModelEditorToolItem(BaseEditorToolItem toolItem)
        {
            this.ToolItem = toolItem;
        }

        public event Action<ViewModelEditorToolItem> IsSelectedChanged;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.ToolItem.Icon);

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.NotifyThisPropertyChanged();
                this.IsSelectedChanged?.Invoke(this);
            }
        }

        public string Name => this.ToolItem.Name;

        public virtual string ShortName => this.ToolItem.ShortName;
    }
}