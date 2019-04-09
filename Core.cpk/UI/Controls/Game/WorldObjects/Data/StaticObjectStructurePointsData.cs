namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.World;

    public struct StaticObjectStructurePointsData
    {
        public readonly StaticObjectPublicState State;

        public readonly IStaticWorldObject StaticWorldObject;

        public readonly float StructurePointsMax;

        public StaticObjectStructurePointsData(
            IStaticWorldObject staticWorldObject,
            float structurePointsMax)
        {
            this.StaticWorldObject = staticWorldObject;
            this.State = staticWorldObject.GetPublicState<StaticObjectPublicState>();
            this.StructurePointsMax = structurePointsMax;
        }

        public IProtoStaticWorldObject ProtoStaticWorldObject
            => this.StaticWorldObject?.ProtoStaticWorldObject;
    }
}