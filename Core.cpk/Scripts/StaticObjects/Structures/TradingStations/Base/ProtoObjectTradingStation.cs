namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectTradingStation
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectTradingStation,
          IInteractableProtoStaticWorldObject
        where TPrivateState : ObjectTradingStationPrivateState, new()
        where TPublicState : ObjectTradingStationPublicState, new()
        where TClientState : ObjectTradingStationClientState, new()
    {
        // displayed when player attempts to interact with a selling station which has no trading lots
        public const string NotificationNothingForSale = "Nothing for sale here";

        // displayed when player attempts to interact with a buying station which has no trading lots
        public const string NotificationNoTradingLots = "No trading lots here";

        public bool HasOwnersList => true;

        public bool IsAutoEnterPrivateScopeOnInteraction =>
            false; // we manually add player to the private scope if she is owner

        public abstract byte LotsCount { get; }

        public abstract byte StockItemsContainerSlotsCount { get; }

        // has light source
        public override BoundsInt ViewBoundsExpansion => new BoundsInt(minX: -3,
                                                                       minY: -3,
                                                                       maxX: 3,
                                                                       maxY: 3);

        public BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData(worldObject));
        }

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            WorldObjectOwnersSystem.ServerOnBuilt(structure, byCharacter);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            TradingStationsSystem.ServerOnDestroy(gameObject);
        }

        public bool SharedCanEditOwners(IStaticWorldObject worldObject, ICharacter byOwner)
        {
            return true;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            // don't use the base implementation as it will not work in PvE
            // (action forbidden if player doesn't have access to the land claim)
            if (character.GetPublicState<ICharacterPublicState>().IsDead)
            {
                return false;
            }

            return this.SharedIsInsideCharacterInteractionArea(character, worldObject, writeToLog);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            var offset = base.SharedGetObjectCenterWorldOffset(worldObject);
            return offset + (0, 0.4); // offset by Y
        }

        void IInteractableProtoStaticWorldObject.ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
            if (WorldObjectOwnersSystem.SharedIsOwner(who, worldObject)
                || CreativeModeSystem.SharedIsInCreativeMode(who))
            {
                Server.World.EnterPrivateScope(who, worldObject);
            }
        }

        void IInteractableProtoStaticWorldObject.ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        protected static void ClientFixTradingStationContentDrawOffset(ClientInitializeData data)
        {
            var clientState = data.ClientState;
            var spriteRenderer = clientState.Renderer;
            var contentRender = clientState.RendererTradingStationContent;
            contentRender.DrawOrderOffsetY = spriteRenderer.PositionOffset.Y
                                             + spriteRenderer.DrawOrderOffsetY
                                             - contentRender.PositionOffset.Y;
        }

        protected abstract BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            var rendererTradingStationContent = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                spritePivotPoint: Vector2D.Zero);

            data.ClientState.RendererTradingStationContent = rendererTradingStationContent;

            data.ClientState.RendererLight = this.ClientCreateLightSource(
                Client.Scene.GetSceneObject(worldObject));

            publicState.ClientSubscribe(_ => _.Mode,
                                        _ => RefreshAppearance(),
                                        data.ClientState);

            foreach (var lot in publicState.Lots)
            {
                if (lot == null)
                {
                    continue;
                }

                lot.ClientSubscribe(_ => _.State,
                                    _ => RefreshAppearance(),
                                    data.ClientState);

                var lastCountAvailable = lot.CountAvailable;
                lot.ClientSubscribe(_ => _.CountAvailable,
                                    newCountAvailable =>
                                    {
                                        var wasAvailable = lastCountAvailable > 0;
                                        var isAvailable = newCountAvailable > 0;
                                        lastCountAvailable = newCountAvailable;

                                        if (wasAvailable != isAvailable)
                                        {
                                            // become available or unavailable
                                            RefreshAppearance();
                                        }
                                    },
                                    data.ClientState);

                lot.ClientSubscribe(_ => _.ProtoItem,
                                    _ => RefreshAppearance(),
                                    data.ClientState);

                lot.ClientSubscribe(_ => _.LotQuantity,
                                    _ => RefreshAppearance(),
                                    data.ClientState);

                lot.ClientSubscribe(_ => _.PriceCoinPenny,
                                    _ => RefreshAppearance(),
                                    data.ClientState);

                lot.ClientSubscribe(_ => _.PriceCoinShiny,
                                    _ => RefreshAppearance(),
                                    data.ClientState);
            }

            RefreshAppearance();

            void RefreshAppearance()
            {
                rendererTradingStationContent.TextureResource
                    = new ProceduralTexture(
                        this.Id + " ID=" + worldObject.Id,
                        isTransparent: true,
                        isUseCache: true,
                        generateTextureCallback:
                        request => this.ClientGenerateProceduralTextureForTradingStationContent(publicState, request));
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            if (worldObject.ClientHasPrivateState
                && !Client.Input.IsKeyHeld(InputKey.Alt))
            {
                // open admin window
                return WindowTradingStationAdmin.Open(
                    new ViewModelWindowTradingStationAdmin(worldObject,
                                                           data.PrivateState,
                                                           publicState));
            }

            // open user window (if has any lots)
            if (publicState.Lots.Any(l => l.State != TradingStationLotState.Disabled))
            {
                return WindowTradingStationUser.Open(
                    new ViewModelWindowTradingStationUser(worldObject,
                                                          publicState));
            }

            CannotInteractMessageDisplay.ShowOn(worldObject,
                                                publicState.Mode == TradingStationMode.StationSelling
                                                    ? NotificationNothingForSale
                                                    : NotificationNoTradingLots);
            return null;
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();
            Api.Assert(this.LotsCount > 0, "Trading station lots count must be > 0");
            this.PrepareProtoTradingStation();
        }

        protected virtual void PrepareProtoTradingStation()
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            WorldObjectOwnersSystem.ServerInitialize(data.GameObject);
            TradingStationsSystem.ServerInitialize(data.GameObject);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);
            TradingStationsSystem.ServerUpdate(data.GameObject);
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (IsServer)
            {
                damagePreMultiplier = LandClaimSystem.ServerAdjustDamageToUnprotectedStrongBuilding(weaponCache,
                                                                                                    targetObject,
                                                                                                    damagePreMultiplier);
            }

            var damage = base.SharedCalculateDamageByWeapon(weaponCache,
                                                            damagePreMultiplier,
                                                            targetObject,
                                                            out obstacleBlockDamageCoef);
            return damage;
        }

        // Generate texture containing text.
        // For text rendering we will use UI.
        private async Task<ITextureResource> ClientGenerateProceduralTextureForTradingStationContent(
            TPublicState publicState,
            ProceduralTextureRequest request)
        {
            var rendering = Client.Rendering;
            var renderingTag = request.TextureName;

            var qualityScaleCoef = rendering.CalculateCurrentQualityScaleCoefWithOffset(-1);
            var scale = 1.0 / qualityScaleCoef;

            var controlWidth = (int)Math.Floor(90 * (this.LotsCount / 2.0));
            var controlHeight = 268;
            // create and prepare UI renderer for the sign text to render
            var control = new ObjectTradingStationDisplayControl
            {
                StationPublicState = publicState,
                Width = controlWidth,
                Height = controlHeight,
                RenderTransformOrigin = new Point(0, 0),
                RenderTransform = new ScaleTransform(scale, scale)
            };

            var textureSize = new Vector2Ushort((ushort)(scale * controlWidth),
                                                (ushort)(scale * controlHeight));

            // create camera and render texture
            var renderTexture = rendering.CreateRenderTexture(renderingTag,
                                                              textureSize.X,
                                                              textureSize.Y);
            var cameraObject = Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -200);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            rendering.CreateUIElementRenderer(
                cameraObject,
                control,
                size: textureSize,
                renderingTag: renderingTag);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true,
                                                                     qualityScaleCoef: qualityScaleCoef);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }
    }

    public abstract class ProtoObjectTradingStation
        : ProtoObjectTradingStation
            <ObjectTradingStationPrivateState,
                ObjectTradingStationPublicState,
                ObjectTradingStationClientState>
    {
    }
}   