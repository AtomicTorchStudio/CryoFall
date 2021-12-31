namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemDroneReservedSlot
        : ProtoItem
            <ItemPrivateState,
                ItemDroneReservedSlot.PublicState,
                EmptyClientState>
    {
        private static readonly Color ColorIcon
            = Color.FromArgb(0x99, 0xCC, 0xCC, 0xCC);

        private static readonly RenderingMaterial IconRenderingMaterial
            = IsClient
                  ? RenderingMaterial.Create(new EffectResource("DefaultDrawEffect"))
                  : null;

        public override string Description => "This slot is temporarily reserved for the drone to return.";

        public override ushort MaxItemsPerStack => 1;

        public override string Name => "Reserved slot";

        public static void ServerSetup(IItem item, IProtoItemDrone protoItemDrone)
        {
            GetPublicState(item).ProtoItemDrone = protoItemDrone;
        }

        public override ITextureResource ClientGetIcon(IItem item)
        {
            var protoItemDrone = GetPublicState(item).ProtoItemDrone;
            if (protoItemDrone is null)
            {
                return null;
            }

            var originalIcon = protoItemDrone.Icon;
            return new ProceduralTexture(this.ShortId + "Icon",
                                         ClientGenerateIcon,
                                         dependsOn: new[] { originalIcon },
                                         isTransparent: true,
                                         isUseCache: true,
                                         data: originalIcon);
        }

        public override void ServerOnItemContainerSlotChanged(IItem item, ICharacter byCharacter)
        {
            var container = item.Container;
            switch (container.Owner)
            {
                case ICharacter:
                case IDynamicWorldObject { ProtoGameObject: IProtoDrone }:
                    // moved to character or drone (as expected)
                    return;

                default:
                    // moved somewhere else - just destroy it and the drone will create a replacement when necessary
                    ServerTimersSystem.AddAction(0,
                                                 () => Api.Server.Items.DestroyItem(item));
                    break;
            }
        }

        protected override ITextureResource PrepareIcon()
        {
            // we have a custom icon provider for this class 
            return null;
        }

        private static async Task<ITextureResource> ClientGenerateIcon(ProceduralTextureRequest request)
        {
            var sourceTextureResource = (ITextureResource)request.Data;
            var renderingTag = request.TextureName;

            var rendering = Client.Rendering;
            var textureSize = await rendering.GetTextureSize(sourceTextureResource);
            var textureWidth = textureSize.X;
            var textureHeight = textureSize.Y;

            var cameraObject = Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag: renderingTag,
                                                drawOrder: -10);

            var renderTexture = rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTexture;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var spriteRenderer = rendering.CreateSpriteRenderer(cameraObject,
                                                                sourceTextureResource,
                                                                renderingTag: renderingTag,
                                                                // draw down
                                                                spritePivotPoint: (0, 1));
            spriteRenderer.Color = ColorIcon;
            spriteRenderer.RenderingMaterial = IconRenderingMaterial;

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: false,
                                                                     customName: renderingTag);
            renderTexture.Dispose();
            request.ThrowIfCancelled();

            return generatedTexture;
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            public IProtoItemDrone ProtoItemDrone { get; set; }
        }
    }
}