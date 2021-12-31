namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
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

        private readonly ICamera2D camera;

        private readonly ICharacter character;

        private readonly string renderingTag;

        private readonly float textureHeight;

        private readonly float textureWidth;

        private IClientItemsContainer equipmentContainer;

        private bool? isActive;

        private bool isNeedRefreshEquipment;

        private IProtoItem overrideProtoItem;

        private ProtoCharacterSkeleton protoCharacterSkeleton;

        private IRenderTarget2D renderTarget2D;

        private IClientSceneObject sceneObjectCamera;

        private IClientSceneObject sceneObjectSkeleton;

        private IComponentSkeleton skeletonRenderer;

        public ViewModelInventorySkeletonViewData(
            ICharacter character,
            IClientSceneObject sceneObjectCamera,
            IClientSceneObject sceneObjectSkeleton,
            ICamera2D camera,
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
            this.camera = camera;

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
            var clientState = PlayerCharacter.GetClientState(character);
            clientState.ClientSubscribe(
                _ => _.LastEquipmentContainerHash,
                _ => this.OnNeedRefreshEquipment(),
                this);

            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.ClientSubscribe(
                _ => _.IsHeadEquipmentHiddenForSelfAndPartyMembers,
                _ => this.OnNeedRefreshEquipment(),
                this);

            //this.RefreshEquipment();
            this.OnNeedRefreshEquipment();
        }

        public SkeletonResource CurrentSkeletonResource
        {
            get => this.skeletonRenderer.CurrentSkeleton;
            set => this.skeletonRenderer.SelectCurrentSkeleton(value, "Idle", true);
        }

        public bool IsActive
        {
            get => this.isActive ?? false;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.camera.DrawMode = value
                                           ? CameraDrawMode.Auto
                                           : CameraDrawMode.Manual;

                if (this.isActive == true
                    && this.isNeedRefreshEquipment)
                {
                    this.isNeedRefreshEquipment = false;
                    this.OnNeedRefreshEquipment();
                }
            }
        }

        public IProtoItem OverrideProtoItem
        {
            get => this.overrideProtoItem;
            set
            {
                if (this.overrideProtoItem == value)
                {
                    return;
                }

                this.overrideProtoItem = value;
                this.OnNeedRefreshEquipment();
            }
        }

        public void RefreshEquipment()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (!this.IsActive)
            {
                // cannot refresh now as not active
                // please note that isNeedRefreshEquipment remains true and
                // on next IsActive=true refresh will be triggered
                return;
            }

            this.isNeedRefreshEquipment = false;
            this.character.ProtoCharacter.SharedGetSkeletonProto(this.character,
                                                                 out var currentProtoCharacterSkeleton,
                                                                 out _);

            if (this.protoCharacterSkeleton != currentProtoCharacterSkeleton)
            {
                if (currentProtoCharacterSkeleton is null)
                {
                    return;
                }

                // proto skeleton changed - destroy current skeleton
                this.protoCharacterSkeleton = (ProtoCharacterSkeleton)currentProtoCharacterSkeleton;
                if (this.skeletonRenderer is not null)
                {
                    this.skeletonRenderer.Destroy();
                    this.skeletonRenderer = null;
                }
            }

            if (this.skeletonRenderer is null)
            {
                var scale = Math.Min(0.125 * this.textureWidth / 128.0,
                                     0.085 * this.textureHeight / 128.0);
                this.skeletonRenderer = ClientCharacterEquipmentHelper.CreateCharacterSkeleton(
                    this.sceneObjectSkeleton,
                    this.protoCharacterSkeleton,
                    worldScale: scale * this.protoCharacterSkeleton.InventoryScale,
                    spriteQualityOffset: -1);

                if (this.skeletonRenderer is null)
                {
                    // failed to create the skeleton renderer (spectator?)
                    return;
                }

                this.skeletonRenderer.RenderingTag = this.renderingTag;
            }

            this.skeletonRenderer.PositionOffset = (this.textureWidth / 2.0, -this.textureHeight * 0.925)
                                                   + this.protoCharacterSkeleton.InventoryOffset;

            /*if (this.overrideProtoItem is null
                    or (not IProtoItemWeaponRanged and not IProtoItemVehicleWeapon))
            {
                this.skeletonRenderer.RemoveAnimationTrack(trackIndex: AnimationTrackIndexes.ItemAiming);
            }*/
            
            // a better approach is to simply reset the skeleton as it will ensure proper active attachments
            this.skeletonRenderer.ResetSkeleton();

            var skeletonComponents = new List<IClientComponent>();
            ClientCharacterEquipmentHelper.SetupSkeletonEquipmentForCharacter(
                this.character,
                this.equipmentContainer,
                this.skeletonRenderer,
                this.protoCharacterSkeleton,
                skeletonComponents,
                isPreview: true,
                overrideProtoItemEquipment: this.overrideProtoItem as IProtoItemEquipment);

            if (this.overrideProtoItem
                    is IProtoItemWithCharacterAppearance overrideProtoItemInHand
                    and not IProtoItemVehicleWeapon)
            {
                // add visuals for skinned weapon or tool
                overrideProtoItemInHand.ClientSetupSkeleton(null,
                                                            this.character,
                                                            this.protoCharacterSkeleton,
                                                            this.skeletonRenderer,
                                                            skeletonComponents);

                if (overrideProtoItemInHand is IProtoItemWeapon protoItemWeapon)
                {
                    // offset the character preview to right in order to give enough space for the weapon
                    var scale = this.textureWidth / 128.0;
                    this.skeletonRenderer.PositionOffset
                        += (scale * protoItemWeapon.SkeletonPreviewOffsetX / 9.0, 0);
                    this.RefreshAnimationTrack();
                }
            } 

            // we don't need these extra components in inventory view
            foreach (var skeletonComponent in skeletonComponents)
            {
                skeletonComponent.Destroy();
            }
        }

        public void SetFrontView()
        {
            if (this.protoCharacterSkeleton?.SkeletonResourceFront is null)
            {
                return;
            }

            this.CurrentSkeletonResource = this.protoCharacterSkeleton.SkeletonResourceFront;
            this.RefreshAnimationTrack();
        }

        public void ToggleView()
        {
            if (this.protoCharacterSkeleton?.SkeletonResourceFront is null)
            {
                return;
            }

            this.CurrentSkeletonResource =
                this.CurrentSkeletonResource != this.protoCharacterSkeleton.SkeletonResourceFront
                    ? this.protoCharacterSkeleton.SkeletonResourceFront
                    : this.protoCharacterSkeleton.SkeletonResourceBack;
            this.RefreshAnimationTrack();
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
            this.sceneObjectCamera = null;
            this.sceneObjectSkeleton.Destroy();
            this.sceneObjectSkeleton = null;
            this.skeletonRenderer = null;
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

        private void RefreshAnimationTrack()
        {
            if (this.overrideProtoItem
                    is IProtoItemWeaponRanged protoItemWeaponRanged
                    and not IProtoItemVehicleWeapon
                && protoItemWeaponRanged.CharacterAnimationAimingName is { } aimingAnimationName)
            {
                this.skeletonRenderer.SetAnimationFrame(
                    trackIndex: AnimationTrackIndexes.ItemAiming,
                    animationName: aimingAnimationName,
                    timePositionFraction: this.skeletonRenderer.CurrentSkeleton
                                          == this.protoCharacterSkeleton.SkeletonResourceFront
                                              ? 0.75f
                                              : 0.25f);
            }
        }
    }
}