namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolStaticObjects
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolStaticObjects : BaseEditorTool<EditorToolStaticObjectsItem>
    {
        public override string Name => "Static Objects tool";

        public override int Order => 20;

        public override BaseEditorActiveTool Activate(EditorToolStaticObjectsItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new EditorActiveToolObjectBrush(
                item.ProtoStaticObject,
                tilePositions => this.ClientPlaceStaticObject(tilePositions, item.ProtoStaticObject));
        }

        protected override void SetupFilters(List<EditorToolItemFilter<EditorToolStaticObjectsItem>> filters)
        {
            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Vegetation",
                    this.GetFilterTexture("Vegetation"),
                    _ => _.ProtoStaticObject is IProtoObjectVegetation));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Resources",
                    this.GetFilterTexture("Resources"),
                    _ => _.ProtoStaticObject is IProtoObjectMineral));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Structures",
                    this.GetFilterTexture("Structures"),
                    _ => _.ProtoStaticObject is IProtoObjectStructure protoStructure
                         && !(protoStructure is IProtoObjectFloor)));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Props",
                    this.GetFilterTexture("Props"),
                    _ => _.ProtoStaticObject is ProtoObjectProp));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Floors",
                    this.GetFilterTexture("Floors"),
                    _ => _.ProtoStaticObject is IProtoObjectFloor));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Loot",
                    this.GetFilterTexture("Loot"),
                    _ => _.ProtoStaticObject is IProtoObjectLoot));

            filters.Add(
                new EditorToolItemFilter<EditorToolStaticObjectsItem>(
                    "Misc",
                    this.GetFilterTexture("Misc"),
                    o => !(o.ProtoStaticObject is IProtoObjectStructure)
                         && !(o.ProtoStaticObject is IProtoObjectVegetation)
                         && !(o.ProtoStaticObject is IProtoObjectMineral)
                         && !(o.ProtoStaticObject is ProtoObjectProp)
                         && !(o.ProtoStaticObject is IProtoObjectLoot)));
        }

        protected override void SetupItems(List<EditorToolStaticObjectsItem> items)
        {
            var protoStaticWorldObjects = Api.FindProtoEntities<IProtoStaticWorldObject>();

            foreach (var prototype in protoStaticWorldObjects
                                      .OrderBy(t => t.Kind)
                                      .ThenBy(t => t.ShortId))
            {
                if (!(prototype is ObjectGroundItemsContainer)
                    && !(prototype is ProtoObjectConstructionSite)
                    && !(prototype is ObjectCorpse))
                {
                    items.Add(new EditorToolStaticObjectsItem(prototype));
                }
            }
        }

        private void ClientPlaceStaticObject(
            List<Vector2Ushort> tilePositions,
            IProtoStaticWorldObject protoStaticWorldObject)
        {
            var tilePosition = tilePositions[0];
            if (Client.World.GetTile(tilePosition)
                      .StaticObjects.Any(so => so.ProtoStaticWorldObject == protoStaticWorldObject))
            {
                return;
            }

            EditorClientSystem.DoAction(
                $"Place object \"{protoStaticWorldObject.Name}\"",
                onDo: () => this.CallServer(
                          _ => _.ServerRemote_PlaceStaticObject(protoStaticWorldObject, tilePosition)),
                onUndo: () => this.CallServer(_ => _.ServerRemote_Destroy(protoStaticWorldObject, tilePosition)));
        }

        private ITextureResource GetFilterTexture(string textureName)
        {
            return new TextureResource($"{this.ToolTexturesPath}Filters/{textureName}.png");
        }

        private void ServerRemote_Destroy(IProtoStaticWorldObject protoStaticWorldObject, Vector2Ushort tilePosition)
        {
            var worldService = Server.World;
            var tile = worldService.GetTile(tilePosition);
            foreach (var staticObject in tile.StaticObjects
                                             .Where(so => so.ProtoStaticWorldObject == protoStaticWorldObject)
                                             .ToList())
            {
                worldService.DestroyObject(staticObject);
            }
        }

        private void ServerRemote_PlaceStaticObject(
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort tilePosition)
        {
            if (!protoStaticWorldObject.CheckTileRequirements(tilePosition, character: null, logErrors: false))
            {
                // cannot spawn here
                return;
            }

            var worldObject = Server.World.CreateStaticWorldObject(protoStaticWorldObject, tilePosition);
            if (protoStaticWorldObject is IProtoObjectVegetation protoVegetation)
            {
                protoVegetation.ServerSetFullGrown(worldObject);
            }
        }
    }
}