namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorToolItemFilter : BaseViewModel
    {
        public readonly EditorToolItemFilter Filter;

        public Action<ViewModelEditorToolItemFilter> OnIsSelectedChanged;

        private bool isSelected;

        public ViewModelEditorToolItemFilter(EditorToolItemFilter filter)
        {
            this.Filter = filter;
        }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.Filter.Icon);

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
                this.OnIsSelectedChanged?.Invoke(this);
            }
        }

        public string Name => this.Filter.Name;
    }
}