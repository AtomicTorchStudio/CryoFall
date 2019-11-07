namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoWorldObjectCustomInteractionCursor : IProtoStaticWorldObject
    {
        CursorId GetInteractionCursorId(bool isCanInteract);
    }
}