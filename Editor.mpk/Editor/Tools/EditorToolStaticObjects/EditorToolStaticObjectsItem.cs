namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolStaticObjects
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class EditorToolStaticObjectsItem : BaseEditorToolItem
    {
        public readonly IProtoStaticWorldObject ProtoStaticObject;

        public EditorToolStaticObjectsItem(IProtoStaticWorldObject protoStaticObject)
            : base(protoStaticObject.Name,
                   protoStaticObject.ShortId,
                   displayShortName: false)
        {
            this.ProtoStaticObject = protoStaticObject;
        }

        public override ITextureResource Icon => this.ProtoStaticObject.Icon;
    }
}