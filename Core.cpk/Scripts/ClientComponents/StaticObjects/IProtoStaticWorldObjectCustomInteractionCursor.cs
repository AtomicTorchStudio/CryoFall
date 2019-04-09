namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoStaticWorldObjectCustomInteractionCursor : IProtoStaticWorldObject
    {
        CursorId GetInteractionCursorId(bool isCanInteract);
    }
}