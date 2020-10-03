namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolStaticObjects
{
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorToolItemStaticObject : ViewModelEditorToolItem
    {
        private int count;

        public ViewModelEditorToolItemStaticObject(EditorToolStaticObjectsItem toolItem)
            : base(toolItem)
        {
            this.count = Api.Client.World.CountObjectsOfProto(toolItem.ProtoStaticObject);
        }

        public int Count
        {
            get => this.count;
            set
            {
                if (this.count == value)
                {
                    return;
                }

                this.count = value;
                this.NotifyPropertyChanged(nameof(this.ShortName));
            }
        }

        public override string ShortName => this.count.ToString();
    }
}