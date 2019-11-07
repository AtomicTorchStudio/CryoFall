namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal class ClientComponentMuzzleFlash : ClientComponent
    {
        private double animationDuration;

        private ICharacter character;

        private ClientComponentSpriteSheetAnimator componentAnimatorFlash;

        private ClientComponentSpriteSheetAnimator componentAnimatorSmoke;

        private IMuzzleFlashDescriptionReadOnly description;

        private bool isRenderingEnabled;

        private uint lastUpdateClientFrame;

        private double lightDuration;

        private BaseClientComponentLightSource lightSource;

        private IProtoItemWeaponRanged protoWeapon;

        private IComponentSkeleton skeletonRenderer;

        private IComponentSpriteRenderer spriteRendererFlash;

        private IComponentSpriteRenderer spriteRendererSmoke;

        private double time;

        public void Setup(
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            IProtoItemWeaponRanged protoWeapon)
        {
            byte flashAtlasRow = 0,
                 smokeAtlasRow = 1;

            this.character = character;
            this.skeletonRenderer = skeletonRenderer;
            this.protoWeapon = protoWeapon;
            this.description = protoWeapon.MuzzleFlashDescription;

            var muzzleFlashTextureAtlas = this.description.TextureAtlas;
            this.animationDuration = this.description.TextureAnimationDurationSeconds;
            this.lightDuration = this.description.LightDurationSeconds;

            // create light renderer
            this.lightSource = ClientLighting.CreateLightSourceSpot(
                this.SceneObject,
                color: this.description.LightColor,
                spritePivotPoint: (0.5, 0.5),
                size: (float)this.description.LightPower,
                // we don't want to display nickname/healthbar for the firing character, it's too quick anyway
                logicalSize: 0);

            this.CreateSpriteRendererAndAnimator(
                out this.spriteRendererSmoke,
                out this.componentAnimatorSmoke,
                smokeAtlasRow,
                protoWeapon,
                muzzleFlashTextureAtlas);

            this.CreateSpriteRendererAndAnimator(
                out this.spriteRendererFlash,
                out this.componentAnimatorFlash,
                flashAtlasRow,
                protoWeapon,
                muzzleFlashTextureAtlas);

            this.Update(deltaTime: 0);
        }

        public override void Update(double deltaTime)
        {
            var clientFrameNumber = Client.Core.ClientFrameNumber;
            if (this.lastUpdateClientFrame == clientFrameNumber)
            {
                // update in current frame is already done, don't progress now
                return;
            }

            this.lastUpdateClientFrame = clientFrameNumber;

            if (!this.skeletonRenderer.IsEnabled)
            {
                this.Destroy();
                return;
            }

            if (!this.skeletonRenderer.IsReady)
            {
                this.SetComponentsState(isRendering: false);
                return;
            }

            if (!this.isRenderingEnabled)
            {
                this.SetComponentsState(isRendering: true);
            }

            if (!this.spriteRendererFlash.IsReady
                || !this.spriteRendererSmoke.IsReady)
            {
                deltaTime = 0;
            }

            this.time += deltaTime;

            if (this.time > this.lightDuration)
            {
                // light duration completed
                this.lightSource.IsEnabled = false;
            }

            if (this.time > this.animationDuration)
            {
                // muzzle flash completed
                this.Destroy();
                return;
            }

            // calculate light opacity
            this.lightSource.Opacity = (this.lightDuration - this.time) / this.lightDuration;

            // calculate sprite position offset

            var protoSkeleton = this.character.GetClientState<BaseCharacterClientState>().CurrentProtoSkeleton;
            var slotName = protoSkeleton.SlotNameItemInHand;
            var weaponSlotScreenOffset = this.skeletonRenderer.GetSlotScreenOffset(attachmentName: slotName);

            var muzzleFlashTextureOffset = this.description.TextureScreenOffset;
            var boneWorldPosition = this.skeletonRenderer.TransformSlotPosition(
                slotName,
                weaponSlotScreenOffset + (Vector2F)muzzleFlashTextureOffset,
                out var worldRotationAngleRad);

            var worldDrawPosition = boneWorldPosition - this.character.Position;

            this.spriteRendererFlash.PositionOffset
                = this.spriteRendererSmoke.PositionOffset
                      = worldDrawPosition;

            this.spriteRendererFlash.RotationAngleRad
                = this.spriteRendererSmoke.RotationAngleRad
                      = worldRotationAngleRad;

            // to ensure muzzle flash rendering right over skeleton renderer we apply this draw offset
            // TODO: consider rendering of muzzle flash right inside the skeleton renderer for proper Z-index
            // (as muzzle flash need sometimes to be rendered behind the skeleton)
            this.spriteRendererFlash.DrawOrderOffsetY
                = this.spriteRendererSmoke.DrawOrderOffsetY
                      = -worldDrawPosition.Y + this.skeletonRenderer.DrawOrderOffsetY;

            var lightTextureOffset = this.description.LightScreenOffsetRelativeToTexture;
            boneWorldPosition = this.skeletonRenderer.TransformSlotPosition(
                slotName,
                weaponSlotScreenOffset
                + ((float)(muzzleFlashTextureOffset.X + lightTextureOffset.X),
                   (float)(muzzleFlashTextureOffset.Y + lightTextureOffset.Y)),
                out _);

            var lightWorldDrawPosition = boneWorldPosition - this.character.Position;
            this.lightSource.PositionOffset = lightWorldDrawPosition;

            this.componentAnimatorFlash.ForceUpdate(deltaTime);
            this.componentAnimatorSmoke.ForceUpdate(deltaTime);

            // visualize muzzle flash position
            //ClientComponentPhysicsSpaceVisualizer.ProcessServerDebugPhysicsTesting(
            //    new CircleShape(boneWorldPosition, radius: 0.3, collisionGroup: CollisionGroups.Default));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.lightSource.Destroy();
            this.spriteRendererFlash.Destroy();
            this.spriteRendererSmoke.Destroy();
            this.componentAnimatorFlash.Destroy();
            this.componentAnimatorSmoke.Destroy();
        }

        private void CreateSpriteRendererAndAnimator(
            out IComponentSpriteRenderer spriteRenderer,
            out ClientComponentSpriteSheetAnimator componentAnimatorFlash,
            byte atlasRow,
            IProtoItemWeaponRanged protoWeapon,
            TextureAtlasResource muzzleFlashTextureAtlas)
        {
            var animationFrameDurationSeconds = this.animationDuration
                                                / (double)muzzleFlashTextureAtlas.AtlasSize.ColumnsCount;

            // create sprite renderer
            spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResource: TextureResource.NoTexture,
                // draw in the same layer as the skeleton renderer
                // (cannot do Default+1 here as it will produce wrong result over other objects)
                drawOrder: DrawOrder.Default,
                rotationAngleRad: 0,
                // align sprite by left side and centered vertical
                spritePivotPoint: (0, 0.5),
                scale: (float)this.description.TextureScale);

            // create animator for sprite renderer
            componentAnimatorFlash = this.SceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimatorFlash.Setup(
                spriteRenderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    muzzleFlashTextureAtlas,
                    onlySpecificRow: atlasRow),
                animationFrameDurationSeconds,
                isManualUpdate: true);
        }

        private void SetComponentsState(bool isRendering)
        {
            this.isRenderingEnabled = isRendering;
            this.lightSource.IsEnabled = isRendering;
            this.spriteRendererFlash.IsEnabled = isRendering;
            this.spriteRendererSmoke.IsEnabled = isRendering;
            this.componentAnimatorFlash.IsEnabled = isRendering;
            this.componentAnimatorSmoke.IsEnabled = isRendering;

            if (isRendering)
            {
                this.componentAnimatorFlash.ForceUpdate(0);
                this.componentAnimatorSmoke.ForceUpdate(0);
            }
        }
    }
}