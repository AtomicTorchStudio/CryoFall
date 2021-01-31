namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
    [PrepareOrder(afterType: typeof(LandClaimSystem.BootstrapperLandClaimSystem))]
    [PrepareOrder(afterType: typeof(WorldMapResourceMarksSystem.Bootstrapper))]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class BootstrapperServerWorld : BaseBootstrapper
    {
        public const int MapVersion = 296;

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
            var allObjects = new List<IStaticWorldObject>(capacity: 100000);

            // this method is super slow but essential as other approach will not work in bootstrapper
#pragma warning disable 618
            Server.World.GetStaticWorldObjects(allObjects);
#pragma warning restore 618

            // destroy all props and other map objects before updating the world as it will load such objects again
            foreach (var worldObject in allObjects)
            {
                var protoGameObject = worldObject.ProtoGameObject;
                if (protoGameObject is IProtoObjectStructure
                    && protoGameObject is not ProtoObjectGateRuins)
                {
                    // player-built structures are not removed
                    continue;
                }

                if (protoGameObject is IProtoObjectPlant)
                {
                    // farm plants considered player structures (alas we cannot determine player-planted trees)
                    continue;
                }

                Server.World.DestroyObject(worldObject);
            }

            Server.World.UpdateWorld(new ServerMapResource(GetInitialMapName()));
        }
    }
}