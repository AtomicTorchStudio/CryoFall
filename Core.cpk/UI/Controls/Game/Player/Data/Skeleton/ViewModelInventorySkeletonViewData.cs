namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;

    public class ViewModelInventorySkeletonViewData : BaseViewModel
    {
        public readonly ImageBrush ImageBrush;

        private readonly ICharacter character;

        private readonly PlayerCharacterClientState clientState;

        private readonly string renderingTag;

        private readonly IClientSceneObject sceneObjectCamera;

        private readonly IClientSceneObject sceneObjectSkeleton;

        private readonly float textureHeight;

        private readonly float textureWidth;

        private IComponentSkeleton currentSkeleton;

        private IClientItemsContainer equipmentContainer;

        private bool isNeedRefreshEquipment;

        private ProtoCharacterSkeleton protoCharacterSkeleton;

        private IRenderTarget2D renderTarget2D;

        public ViewModelInventorySkeletonViewData(
            ICharacter character,
            IClientSceneObject sceneObjectCamera,
            IClientSceneObject sceneObjectSkeleton,
            IRenderTarget2D renderTarget2D,
            string renderingTag,
            float textureWidth,
            float textureHeight)
        {
            this.character = character;

            this.equipmentContainer = (IClientItemsContainer)character
                                                             .GetPublicState<ICharacterPublicStateWithEquipment>()
                                                             .ContainerEquipment;

            this.equipmentContainer.StateHashChanged += this.EquipmentContainerStateHashChangedHandler;

            this.sceneObjectCamera = sceneObjectCamera;
            this.sceneObjectSkeleton = sceneObjectSkeleton;

            this.renderTarget2D = renderTarget2D;
            this.renderingTag = renderingTag;
            this.textureWidth = textureWidth;
            this.textureHeight = textureHeight;
            this.ImageBrush = InventorySkeletonViewHelper.Client.UI.CreateImageBrushForRenderTarget(
                renderTarget2D);

            this.ImageBrush.Stretch = Stretch.Uniform;

            // subscribe on change of the face
            // commented out - no need, the skeleton will be rebuilt completely
            //var publicState = PlayerCharacter.GetPublicState(character);
            //publicState.ClientSubscribe(
            //    _ => _.FaceStyle,
            //    _ => this.OnNeedRefreshEquipment(),
            //    this);

            // subscribe on change of the equipment
            this.clientState = PlayerCharacter.GetClientState(character);
            this.clientState.ClientSubscribe(
                _ => _.LastEquipmentContainerHash,
                _ => this.OnNeedRefreshEquipment(),
                this);

            this.RefreshEquipment();
        }

        public SkeletonResource CurrentSkeletonResource
        {
            get => this.currentSkeleton.CurrentSkeleton;
            set => this.currentSkeleton.SelectCurrentSkeleton(value, "Idle", true);
        }

        public void RefreshEquipment()
        {
            this.isNeedRefreshEquipment = false;

            if (this.protoCharacterSkeleton != this.clientState.CurrentProtoSkeleton)
            {
                if (this.clientState.CurrentProtoSkeleton == null)
                {
                    return;
                }

                // proto skeleton changed - destroy current skeleton
                this.protoCharacterSkeleton = this.clientState.CurrentProtoSkeleton;
                if (this.currentSkeleton != null)
                {
                    this.currentSkeleton.Destroy();
                    this.currentSkeleton = null;
                }
            }

            if (this.currentSkeleton == null)
            {
                var scale = this.textureWidth / 128;
                this.currentSkeleton = ClientCharacterEquipmentHelper.CreateCharacterSkeleton(
                    this.sceneObjectSkeleton,
                    this.protoCharacterSkeleton,
                    worldScale: 0.125 * scale,
                    spriteQualityOffset: -1);

                if (this.currentSkeleton == null)
                {
                    // failed to create the skeleton renderer (spectator?)
                    return;
                }

                this.currentSkeleton.PositionOffset = (this.textureWidth / 2.0,
                                                       -this.textureHeight * 0.925);
                this.currentSkeleton.RenderingTag = this.renderingTag;
            }

            var skeletonComponents = new List<IClientComponent>();
            ClientCharacterEquipmentHelper.SetupSkeletonEquipmentForCharacter(
                this.character,
                this.equipmentContainer,
                this.currentSkeleton,
                this.protoCharacterSkeleton,
                skeletonComponents);

            // we don't need these extra components in inventory view
            foreach (var skeletonComponent in skeletonComponents)
            {
                skeletonComponent.Destroy();
            }
        }

        public void ToggleView()
        {
            if (this.protoCharacterSkeleton?.SkeletonResourceFront == null)
            {
                return;
            }

            this.CurrentSkeletonResource =
                this.CurrentSkeletonResource != this.protoCharacterSkeleton.SkeletonResourceFront
                    ? this.protoCharacterSkeleton.SkeletonResourceFront
                    : this.protoCharacterSkeleton.SkeletonResourceBack;
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.equipmentContainer.StateHashChanged -= this.EquipmentContainerStateHashChangedHandler;
            this.equipmentContainer = null;
            this.ImageBrush.ImageSource = null;
            this.renderTarget2D.Dispose();
            this.renderTarget2D = null;
            this.sceneObjectCamera.Destroy();
            this.sceneObjectSkeleton.Destroy();
            this.currentSkeleton = null;
        }

        private void EquipmentContainerStateHashChangedHandler()
        {
            this.OnNeedRefreshEquipment();
        }

        private void OnNeedRefreshEquipment()
        {
            if (this.isNeedRefreshEquipment)
            {
                return;
            }

            this.isNeedRefreshEquipment = true;
            ClientTimersSystem.AddAction(
                // next frame
                0,
                () =>
                {
                    if (this.isNeedRefreshEquipment)
                    {
                        this.RefreshEquipment();
                    }
                });
        }
    }
}