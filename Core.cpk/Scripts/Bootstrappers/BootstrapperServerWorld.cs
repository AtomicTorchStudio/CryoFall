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
    [PrepareOrder(afterType: typeof(WorldMapResourceMarksSystem.BootstrapperWorldMapResourcesSystem))]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class BootstrapperServerWorld : BaseBootstrapper
    {
        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            if (!Api.Server.Database.TryGet("Core", "IsInitialMapLoaded", out bool isInitialMapLoaded)
                || !isInitialMapLoaded)
            {
                Api.Server.Database.Set("Core", "IsInitialMapLoaded", true);
                LoadMap();
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
            var worldObjects = Server.World.FindStaticWorldObjectsOfProto<IProtoObjectVegetation>();
            foreach (var worldObject in worldObjects)
            {
                ((IProtoObjectVegetation)worldObject.ProtoWorldObject)
                    .ServerSetFullGrown(worldObject);
            }

            // destroy all land claim buildings as they don't work as intended when simply spawned
            worldObjects = Server.World.FindStaticWorldObjectsOfProto<IProtoObjectLandClaim>();
            foreach (var worldObject in Api.Shared.WrapInTempList(worldObjects))
            {
                Server.World.DestroyObject(worldObject);
            }
        }
    }
}