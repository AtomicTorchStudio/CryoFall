namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

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
        public override bool IsRelocatable => true;

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            // destroy all the plants over this plant pot
            foreach (var tile in gameObject.OccupiedTiles)
            {
                foreach (var staticWorldObject in Api.Shared.WrapInTempList(tile.StaticObjects).EnumerateAndDispose())
                {
                    if (staticWorldObject.ProtoStaticWorldObject is IProtoObjectPlant)
                    {
                        Server.World.DestroyObject(staticWorldObject);
                    }
                }
            }

            base.ServerOnDestroy(gameObject);
        }
    }

    public abstract class ProtoObjectPlantPot
        : ProtoObjectPlantPot
            <StructurePrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}