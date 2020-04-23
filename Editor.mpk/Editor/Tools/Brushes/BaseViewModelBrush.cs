namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseViewModelBrush : BaseViewModel
    {
        private ViewModelEnum<EditorBrushShape> selectedBrushShape;

        private int selectedBrushSize;

        protected BaseViewModelBrush(
            int defaultBrushSize = 3,
            int maxBrushSize = 20)
        {
            this.BrushSizes = Enumerable.Range(1, maxBrushSize).ToArray();
            this.selectedBrushSize = defaultBrushSize;

            this.BrushShapes = EnumHelper.EnumValuesToViewModel<EditorBrushShape>();
            this.selectedBrushShape = this.BrushShapes[0];
        }

        public event Action BrushSettingsChanged;

        public ViewModelEnum<EditorBrushShape>[] BrushShapes { get; }

        public int[] BrushSizes { get; }

        public ViewModelEnum<EditorBrushShape> SelectedBrushShape
        {
            get => this.selectedBrushShape;
            set
            {
                if (this.selectedBrushShape == value)
                {
                    return;
                }

                this.selectedBrushShape = value;
                this.NotifyThisPropertyChanged();
                this.BrushSettingsChanged?.Invoke();
            }
        }

        public int SelectedBrushSize
        {
            get => this.selectedBrushSize;
            set
            {
                if (this.selectedBrushSize == value)
                {
                    return;
                }

                this.selectedBrushSize = value;
                this.NotifyThisPropertyChanged();
                this.BrushSettingsChanged?.Invoke();
            }
        }
    }
}