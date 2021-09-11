namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    public abstract class ProtoObjectPlantFlower
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectPlant<TPrivateState, TPublicState, TClientState>,
          IProtoObjectPlantFlower
        where TPrivateState : PlantPrivateState, new()
        where TPublicState : PlantPublicState, new()
        where TClientState : PlantClientState, new()
    {
    }

    public abstract class ProtoObjectPlantFlower
        : ProtoObjectPlantFlower<
            PlantPrivateState,
            PlantPublicState,
            PlantClientState>
    {
    }
}