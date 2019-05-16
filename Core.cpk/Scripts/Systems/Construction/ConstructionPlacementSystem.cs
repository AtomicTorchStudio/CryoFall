namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConstructionPlacementSystem : ProtoSystem<ConstructionPlacementSystem>, IMenu
    {
        public const string NotificationCannotBuild_Title = "Cannot build there";

        private const bool AllowInstantPlacementInCreativeMode = false;

        private const double MaxDistanceToBuild = 5;

        private static ClientComponentObjectPlacementHelper componentObjectPlacementHelper;

        private static IProtoObjectStructure currentSelectedProtoConstruction;

        public delegate void StructureBuiltDelegate(ICharacter character, IStaticWorldObject structure);

        public static event StructureBuiltDelegate ServerStructureBuilt;

        public event Action IsOpenedChanged;

        public static bool IsObjectPlacementComponentEnabled => componentObjectPlacementHelper?.IsEnabled ?? false;

        public bool IsOpened
        {
            get
            {
                var window = WindowConstructionMenu.Instance;
                return window != null
                       && (window.WindowState == GameWindowState.Opened
                           || window.WindowState == GameWindowState.Opening);
            }
        }

        public override string Name => "Construction placement system";

        public static void ClientDisableConstructionPlacement()
        {
            componentObjectPlacementHelper?.SceneObject.Destroy();
            componentObjectPlacementHelper = null;
        }

        public static void ClientToggleConstructionMenu()
        {
            if (ClientCloseConstructionMenu())
            {
                // just closed the construction menu
                return;
            }

            WindowConstructionMenu.Open(
                onStructureProtoSelected:
                selectedProtoStructure =>
                {
                    ClientEnsureConstructionToolIsSelected();
                    currentSelectedProtoConstruction = selectedProtoStructure;

                    componentObjectPlacementHelper = Client.Scene
                                                           .CreateSceneObject(
                                                               "ConstructionHelper",
                                                               Vector2D.Zero)
                                                           .AddComponent<
                                                               ClientComponentObjectPlacementHelper>();

                    // repeat placement for held button only for walls, floor and farms
                    var isRepeatCallbackIfHeld = selectedProtoStructure is IProtoObjectWall
                                                 || selectedProtoStructure is IProtoObjectFloor
                                                 || selectedProtoStructure is IProtoObjectFarm;

                    componentObjectPlacementHelper
                        .Setup(selectedProtoStructure,
                               isCancelable: true,
                               isRepeatCallbackIfHeld: isRepeatCallbackIfHeld,
                               isDrawConstructionGrid: true,
                               isBlockingInput: true,
                               validateCanPlaceCallback: ClientValidateCanBuildCallback,
                               placeSelectedCallback: ClientConstructionPlaceSelectedCallback,
                               maxDistance: MaxDistanceToBuild,
                               delayRemainsSeconds: 0.4);
                },
                onClosed: OnStructureSelectWindowOpenedOrClosed);

            OnStructureSelectWindowOpenedOrClosed();
        }

        public static void ServerReplaceConstructionSiteWithStructure(
            IStaticWorldObject worldObject,
            IProtoObjectStructure protoStructure,
            ICharacter byCharacter)
        {
            if (worldObject?.IsDestroyed ?? true)
            {
                throw new Exception("Construction site doesn't exist or already destroyed: " + worldObject);
            }

            var tilePosition = worldObject.TilePosition;

            // destroy construction site
            Server.World.DestroyObject(worldObject);

            // create structure
            var structure = ConstructionSystem.ServerCreateStructure(
                protoStructure,
                tilePosition,
                byCharacter: byCharacter);

            if (byCharacter == null)
            {
                return;
            }

            Instance.ServerOnStructurePlaced(structure, byCharacter);
            Api.SafeInvoke(() => ServerStructureBuilt?.Invoke(byCharacter, structure));
        }

        public void Dispose()
        {
        }

        public void InitMenu()
        {
        }

        public void ServerOnStructurePlaced(IStaticWorldObject structure, ICharacter byCharacter)
        {
            //// it will return empty list because the object is new!
            // var scopedByPlayers = Server.World.GetScopedByPlayers(structure);

            //// Workaround:
            using (var scopedByPlayers = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(byCharacter, scopedByPlayers);
                this.CallClient(scopedByPlayers,
                                _ => _.ClientRemote_OnStructurePlaced(structure.ProtoStaticWorldObject,
                                                                      structure.TilePosition,
                                                                      /*isByCurrentPlayer: */
                                                                      false));
            }

            this.CallClient(byCharacter,
                            _ => _.ClientRemote_OnStructurePlaced(structure.ProtoStaticWorldObject,
                                                                  structure.TilePosition,
                                                                  /*isByCurrentPlayer: */
                                                                  true));
        }

        public void Toggle()
        {
            ClientToggleConstructionMenu();
        }

        protected virtual IStaticWorldObject ServerCreateConstructionSite(
            Vector2Ushort tilePosition,
            IProtoObjectStructure protoStructure,
            ICharacter byCharacter)
        {
            if (protoStructure == null)
            {
                throw new ArgumentNullException(nameof(protoStructure));
            }

            var protoConstructionSite = Api.GetProtoEntity<ObjectConstructionSite>();
            var constructionSite = Server.World.CreateStaticWorldObject(protoConstructionSite, tilePosition);

            var serverState = ObjectConstructionSite.GetPublicState(constructionSite);
            serverState.Setup(protoStructure);

            constructionSite.ProtoStaticWorldObject.SharedCreatePhysics(constructionSite);

            Logger.Important("Construction site created: " + constructionSite);
            protoConstructionSite.ServerOnBuilt(constructionSite, byCharacter);
            Api.SafeInvoke(() => ServerStructureBuilt?.Invoke(byCharacter, constructionSite));
            Api.SafeInvoke(() => SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(
                               constructionSite,
                               isDestroy: false));

            return constructionSite;
        }

        private static bool ClientCloseConstructionMenu()
        {
            if (Instance.IsOpened)
            {
                // construction window is opened
                WindowConstructionMenu.Instance.Close();
                return true;
            }

            // construction window is closed
            if (componentObjectPlacementHelper != null
                && componentObjectPlacementHelper.IsEnabled)
            {
                // construction location selector is active - destroy it and don't open the construction menu
                ClientDisableConstructionPlacement();
                return true;
            }

            return false;
        }

        private static void ClientConstructionPlaceSelectedCallback(Vector2Ushort tilePosition)
        {
            ClientEnsureConstructionToolIsSelected();

            // validate if there are enough required items/resources to build the structure
            Instance.CallServer(_ => _.ServerRemote_PlaceStructure(currentSelectedProtoConstruction, tilePosition));

            if (currentSelectedProtoConstruction is IProtoObjectLandClaim)
            {
                ClientDisableConstructionPlacement();
            }
        }

        private static void ClientEnsureConstructionToolIsSelected()
        {
            var activeItem = ClientHotbarSelectedItemManager.SelectedItem;
            if (activeItem?.ProtoItem is IProtoItemToolToolbox)
            {
                // tool is selected
                return;
            }

            var itemTool = (from item in ClientHotbarSelectedItemManager.ContainerHotbar.Items
                            let protoToolbox = item.ProtoItem as IProtoItemToolToolbox
                            where protoToolbox != null
                            // find best tool
                            orderby protoToolbox.ConstructionSpeedMultiplier descending,
                                item.ContainerSlotId ascending
                            select item).FirstOrDefault();

            if (itemTool == null)
            {
                throw new Exception("Cannot build - the required tool is not available in the hotbar");
            }

            ClientHotbarSelectedItemManager.Select(itemTool);
        }

        private static bool ClientValidateCanBuildCallback(Vector2Ushort tilePosition, bool logErrors)
        {
            var protoStructure = currentSelectedProtoConstruction;

            if (Client.World.GetTile(tilePosition)
                      .StaticObjects
                      .Any(so => so.ProtoStaticWorldObject == protoStructure
                                 || ProtoObjectConstructionSite.SharedIsConstructionOf(so, protoStructure)))
            {
                // this building is already built here
                return false;
            }

            if (!protoStructure.CheckTileRequirements(
                    tilePosition,
                    Client.Characters.CurrentPlayerCharacter,
                    logErrors: logErrors))
            {
                // time requirements are not valid
                return false;
            }

            var configBuild = protoStructure.ConfigBuild;
            if (configBuild.CheckStageCanBeBuilt(Client.Characters.CurrentPlayerCharacter))
            {
                return true;
            }

            //NotificationSystem.ClientShowNotification("Cannot build", "Not enough items.", NotificationColor.Bad);

            // close construction menu
            ClientCloseConstructionMenu();
            return false;
        }

        private static void OnStructureSelectWindowOpenedOrClosed()
        {
            Instance.IsOpenedChanged?.Invoke();
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_CannotBuildTooFar(IProtoStaticWorldObject protoStaticWorldObject)
        {
            NotificationSystem.ClientShowNotification(
                NotificationCannotBuild_Title,
                CoreStrings.Notification_TooFar,
                NotificationColor.Bad,
                protoStaticWorldObject.Icon);
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_OnStructurePlaced(
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort position,
            bool isByCurrentPlayer)
        {
            var soundPreset = protoStaticWorldObject.SharedGetObjectSoundPreset();
            if (isByCurrentPlayer)
            {
                // play 2D sound
                soundPreset.PlaySound(ObjectSound.Place);
            }
            else
            {
                // play 3D sound (at built object)
                soundPreset.PlaySound(ObjectSound.Place,
                                      position.ToVector2D() + protoStaticWorldObject.Layout.Center);
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ServerRemote_PlaceStructure(
            IProtoObjectStructure protoStructure,
            Vector2Ushort tilePosition)
        {
            var character = ServerRemoteContext.Character;

            if (!protoStructure.SharedIsTechUnlocked(character))
            {
                Logger.Error(
                    $"Cannot build {protoStructure} at {tilePosition}: player character doesn't have unlocked tech node for this structure.",
                    character);
                return;
            }

            if (character.TilePosition.TileDistanceTo(tilePosition)
                > MaxDistanceToBuild)
            {
                this.CallClient(character, _ => _.ClientRemote_CannotBuildTooFar(protoStructure));
                return;
            }

            // validate if the structure can be placed there
            if (!protoStructure.CheckTileRequirements(tilePosition, character, logErrors: true))
            {
                return;
            }

            // validate if there are enough required items/resources to build the structure
            var configBuild = protoStructure.ConfigBuild;
            if (!configBuild.CheckStageCanBeBuilt(character))
            {
                Logger.Error(
                    $"Cannot build {protoStructure} at {tilePosition}: player character doesn't have enough resources (or not allowed).",
                    character);
                return;
            }

            var selectedHotbarItem = PlayerCharacter.GetPublicState(character).SelectedHotbarItem;
            if (!(selectedHotbarItem?.ProtoItem is IProtoItemToolToolbox))
            {
                Logger.Error(
                    $"Cannot build {protoStructure} at {tilePosition}: player character doesn't have selected construction tool.",
                    character);
                return;
            }

            // consume required items/resources (for 1 stage)
            configBuild.ServerDestroyRequiredItems(character);

            if (configBuild.StagesCount > 1)
            {
                if (AllowInstantPlacementInCreativeMode
                    && CreativeModeSystem.SharedIsInCreativeMode(character))
                {
                    // instant placement allowed
                }
                else
                {
                    // there are multiple construction stages - spawn and setup a construction site
                    var constructionSite = this.ServerCreateConstructionSite(tilePosition, protoStructure, character);
                    this.ServerOnStructurePlaced(constructionSite, character);
                    return;
                }
            }

            ServerDecalsDestroyHelper.DestroyAllDecals(tilePosition, protoStructure.Layout);

            // there is only one construction stage - simply spawn the structure
            var structure = ConstructionSystem.ServerCreateStructure(
                protoStructure,
                tilePosition,
                character);

            this.ServerOnStructurePlaced(structure, character);
            Api.SafeInvoke(() => ServerStructureBuilt?.Invoke(character, structure));
        }
    }
}