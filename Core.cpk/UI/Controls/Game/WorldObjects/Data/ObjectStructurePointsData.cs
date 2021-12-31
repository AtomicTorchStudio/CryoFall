namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;

    public readonly struct ObjectStructurePointsData
    {
        public readonly IPublicStateWithStructurePoints State;

        public readonly float StructurePointsMax;

        public readonly IWorldObject WorldObject;

        public ObjectStructurePointsData(
            IWorldObject worldObject,
            float structurePointsMax)
        {
            this.WorldObject = worldObject;
            this.State = worldObject.GetPublicState<IPublicStateWithStructurePoints>();
            this.StructurePointsMax = structurePointsMax;
        }

        public IDamageableProtoWorldObject ProtoWorldObject
            => (IDamageableProtoWorldObject)this.WorldObject?.ProtoGameObject;
    }
}