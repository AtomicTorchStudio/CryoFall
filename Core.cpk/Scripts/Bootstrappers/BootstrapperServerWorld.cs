namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
    [PrepareOrder(afterType: typeof(LandClaimSystem.BootstrapperLandClaimSystem))]
    [PrepareOrder(afterType: typeof(WorldMapResourceMarksSystem.Bootstrapper))]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class BootstrapperServerWorld : BaseBootstrapper
    {
        public const int MapVersion = 263;

        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            var db = Api.Server.Database;

            if (!db.TryGet("Core", "IsInitialMapLoaded", out bool isInitialMapLoaded)
                || !isInitialMapLoaded)
            {
                db.Set("Core", "IsInitialMapLoaded", true);
                LoadMap();
                db.Set("Core", "MapCurrentGameVersion", MapVersion);
            }
            else if (!db.TryGet("Core", "MapCurrentGameVersion", out int mapVersion)
                     || MapVersion != mapVersion)
            {
                UpdateMap();
                db.Set("Core", "MapCurrentGameVersion", MapVersion);
            }
        }

        private static string GetInitialMapName()
        {
            return Api.IsEditor
                       ? "Editor.map"
                       : "CryoFall.map";
        }

        private static void LoadMap()
        {
            Server.World.LoadWorld(new ServerMapResource(GetInitialMapName()));

            // set all loaded vegetation objects as full grown
            var worldObjects = Server.World.GetStaticWorldObjectsOfProto<IProtoObjectVegetation>();
            foreach (var worldObject in worldObjects)
            {
                ((IProtoObjectVegetation)worldObject.ProtoWorldObject)
                    .ServerSetFullGrown(worldObject);
            }

            // destroy all land claim buildings as they don't work as intended when simply spawned
            worldObjects = Server.World.GetStaticWorldObjectsOfProto<IProtoObjectLandClaim>();
            foreach (var worldObject in Api.Shared.WrapInTempList(worldObjects).EnumerateAndDispose())
            {
                Server.World.DestroyObject(worldObject);
            }
        }

        private static void UpdateMap()
        {
            Server.World.UpdateWorld(new ServerMapResource(GetInitialMapName()));
        }
    }
}