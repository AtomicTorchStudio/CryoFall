namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Signs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectSign
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectSign,
          IInteractableProtoWorldObject
        where TPrivateState : ObjectWithOwnerPrivateState, new()
        where TPublicState : ObjectSignPublicState, new()
        where TClientState : ObjectSignClientState, new()
    {
        public const string NotificationCannotEditSignWhenNotAreaOwner =
            @"You cannot edit signs
              [br]inside other people's claimed areas.";

        private static readonly int MaxSignTextLength = 50;

        public override string InteractionTooltipText => InteractionTooltipTexts.Write;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        public void ClientSetSignText(IStaticWorldObject worldObjectSign, string signText)
        {
            signText = signText?.Trim();
            SharedValidateSignText(signText);
            this.CallServer(_ => _.ServerRemote_SetSignText(worldObjectSign, signText));
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject, character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            // not the land owner and not in creative mode
            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - not the land owner.",
                    character);

                if (IsClient)
                {
                    this.ClientRemote_OnCannotInteract(worldObject);
                }
                else
                {
                    this.CallClient(
                        character,
                        _ => _.ClientRemote_OnCannotInteract(worldObject));
                }
            }

            return false;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected static void ClientFixSignDrawOffset(ClientInitializeData data)
        {
            var clientState = data.ClientState;
            var spriteRenderer = clientState.Renderer;
            var contentRender = clientState.RendererSignContent;
            contentRender.DrawOrderOffsetY = spriteRenderer.PositionOffset.Y
                                             + spriteRenderer.DrawOrderOffsetY
                                             - contentRender.PositionOffset.Y;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            var rendererSignContent = Client.Rendering.CreateSpriteRenderer(worldObject);
            data.ClientState.RendererSignContent = rendererSignContent;

            publicState.ClientSubscribe(_ => _.Text,
                                        _ => this.ClientRefreshSignRendering(rendererSignContent, publicState),
                                        data.ClientState);

            this.ClientRefreshSignRendering(rendererSignContent, publicState);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowSign.Open(
                new ViewModelWindowSign(data.GameObject));
        }

        protected virtual void ClientRefreshSignRendering(
            IComponentSpriteRenderer rendererSignContent,
            TPublicState publicState)
        {
            rendererSignContent.TextureResource
                = new ProceduralTexture(
                    nameof(ProtoObjectSign) + ": " + publicState.Text,
                    isTransparent: true,
                    isUseCache: true,
                    generateTextureCallback:
                    request => this.ClientGenerateProceduralTextureForText(publicState.Text, request));
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            this.PrepareConstructionConfig(build,
                                           repair,
                                           upgrade,
                                           out category);
        }

        protected abstract void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category);

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.5, 0.2),  (0.25, 0.4))
                .AddShapeRectangle((0.8, 0.4),  (0.1, 1.0), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.8, 0.15), (0.1, 1.2), CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.8, 1.0),  (0.1, 0.4), CollisionGroups.ClickArea);
        }

        private static void SharedValidateSignText(string signText)
        {
            if (string.IsNullOrEmpty(signText))
            {
                return;
            }

            if (signText.Length > MaxSignTextLength)
            {
                throw new Exception($"Max sign text length is {MaxSignTextLength} chars");
            }
        }

        // Generate texture containing text.
        // For text rendering we will use UI.
        private async Task<ITextureResource> ClientGenerateProceduralTextureForText(
            string text,
            ProceduralTextureRequest request)
        {
            var rendering = Client.Rendering;
            var renderingTag = request.TextureName;

            var qualityScaleCoef = rendering.CalculateCurrentQualityScaleCoefWithOffset(-1);
            var scale = 1.0 / qualityScaleCoef;

            var control = new ObjectSignControl()
            {
                Text = text,
                LayoutTransform = new ScaleTransform(scale, scale)
            };

            var controlWidth = control.Width;
            var controlHeight = control.Height;

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

            // create and prepare UI renderer for this text to render
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

        private void ClientRemote_OnCannotInteract(IStaticWorldObject worldObject)
        {
            ClientOnCannotInteract(worldObject,
                                   NotificationCannotEditSignWhenNotAreaOwner,
                                   isOutOfRange: false);
        }

        private void ServerRemote_SetSignText(IStaticWorldObject worldObjectSign, string signText)
        {
            this.VerifyGameObject(worldObjectSign);

            if (!this.SharedCanInteract(ServerRemoteContext.Character, worldObjectSign, writeToLog: true))
            {
                return;
            }

            signText = signText?.Trim();
            SharedValidateSignText(signText);
            signText = ProfanityFilteringSystem.SharedApplyFilters(signText);
            GetPublicState(worldObjectSign).Text = signText;
        }
    }

    public abstract class ProtoObjectSign
        : ProtoObjectSign<ObjectWithOwnerPrivateState, ObjectSignPublicState, ObjectSignClientState>
    {
    }
}