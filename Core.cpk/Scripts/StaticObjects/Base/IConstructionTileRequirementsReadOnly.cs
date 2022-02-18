namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IConstructionTileRequirementsReadOnly
    {
        bool Check(
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort tilePosition,
            ICharacter character,
            out object errorCodeOrMessage,
            bool logErrors);

        bool Check(ConstructionTileRequirements.Context context, out object errorCodeOrMessage);

        ConstructionTileRequirements Clone();
    }
}