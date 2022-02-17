namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectSmallGatherableVegetation
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectGatherableVegetation
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : VegetationPrivateState, new()
        where TPublicState : VegetationPublicState, new()
        where TClientState : VegetationClientState, new()
    {
        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.15);
        }
    }

    public abstract class ProtoObjectSmallGatherableVegetation
        : ProtoObjectSmallGatherableVegetation
            <VegetationPrivateState,
                VegetationPublicState,
                VegetationClientState>
    {
    }
}