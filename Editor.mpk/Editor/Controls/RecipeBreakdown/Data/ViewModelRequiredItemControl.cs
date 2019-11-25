namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelRequiredItemControl : BaseViewModel
    {
        private ProtoItemWithCountFractional protoItemWithCount;

        public ViewModelRequiredItemControl(ProtoItemWithCountFractional protoItemWithCount)
        {
            this.ProtoItemWithCount = protoItemWithCount;
        }

        public ViewModelRequiredItemControl()
        {
        }

        public string CountString { get; private set; }

        public Visibility CountVisibility { get; set; }

        public string Description => this.protoItemWithCount?.ProtoItem.Description;

        public Brush Icon { get; private set; }

        public ProtoItemWithCountFractional ProtoItemWithCount
        {
            get => this.protoItemWithCount;
            set
            {
                if (this.protoItemWithCount == value)
                {
                    return;
                }

                this.protoItemWithCount = value;
                this.Refresh();
            }
        }

        public string Title => this.protoItemWithCount?.ProtoItem.Name;

        public void Refresh()
        {
            this.NotifyPropertyChanged(nameof(this.Title));
            if (this.protoItemWithCount == null)
            {
                this.CountVisibility = Visibility.Collapsed;
                this.Icon = null;
                return;
            }

            this.RefreshCount();
            this.CountVisibility = Visibility.Visible;
            this.Icon = Api.Client.UI.GetTextureBrush(this.protoItemWithCount?.ProtoItem.Icon);
        }

        public void RefreshCount()
        {
            this.CountString = MathHelper.RoundToSignificantDigits(this.protoItemWithCount.Count, 2)
                                         .ToString("0.######");
        }
    }
}