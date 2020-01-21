namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerDecalsDestroyHelper
    {
        private static readonly IWorldServerService World = Api.Server.World;

        public static void DestroyAllDecals(
            Vector2Ushort tilePosition,
            StaticObjectLayoutReadOnly layout)
        {
            Api.ValidateIsServer();

            foreach (var tileOffset in layout.TileOffsets)
            {
                var tile = World.GetTile(tilePosition.X + tileOffset.X,
                                         tilePosition.Y + tileOffset.Y);
                if (!tile.IsValidTile)
                {
                    continue;
                }

                var staticObjects = tile.StaticObjects;
                if (!HasFloorDecals(staticObjects))
                {
                    continue;
                }

                // remove floor decals
                using var tempList = Api.Shared.WrapInTempList(staticObjects);
                foreach (var staticWorldObject in tempList.AsList())
                {
                    if (staticWorldObject.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal)
                    {
                        World.DestroyObject(staticWorldObject);
                    }
                }
            }

            bool HasFloorDecals(ReadOnlyListWrapper<IStaticWorldObject> staticObjects)
            {
                foreach (var staticWorldObject in staticObjects)
                {
                    if (staticWorldObject.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}