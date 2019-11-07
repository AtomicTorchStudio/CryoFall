namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelStructureCategory : BaseViewModel
    {
        public ViewModelStructureCategory(ProtoStructureCategory category)
        {
            this.Title = category.Name;
            this.Category = category;
        }

        public ViewModelStructureCategory(string title)
        {
            this.Title = title;
        }

        public ProtoStructureCategory Category { get; }

        public bool IsEnabled { get; set; } = true;

        public bool IsSelected { get; set; }

        public string Title { get; }
    }
}