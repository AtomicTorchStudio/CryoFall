namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectPlantPot
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectFarm
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectPlantPot
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public override StaticObjectKind Kind => StaticObjectKind.Structure;
    }

    public abstract class ProtoObjectPlantPot
        : ProtoObjectPlantPot
            <StructurePrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}