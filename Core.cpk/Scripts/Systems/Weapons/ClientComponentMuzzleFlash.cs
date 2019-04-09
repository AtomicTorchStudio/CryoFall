namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System.Diagnostics.CodeAnalysis;
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

        private IMuzzleFlashDescriptionReadOnly description;

        private double lightDuration;

        private BaseClientComponentLightSource lightSource;

        private IProtoItemWeaponRanged protoWeapon;

        private IComponentSkeleton skeletonRenderer;

        private IComponentSpriteRenderer spriteRendererFlash;

        private IComponentSpriteRenderer spriteRendererSmoke;

        private double time;

        public ClientComponentMuzzleFlash() : base(isLateUpdateEnabled: true)
        {
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public override void LateUpdate(double deltaTime)
        {
            if (!this.skeletonRenderer.IsReady)
            {
                this.SetComponentsState(isRendering: false);
                return;
            }

            // calculate sprite position offset
            const string boneName = "Weapon";
            var weaponSlotScreenOffset = this.skeletonRenderer.GetSlotScreenOffset(attachmentName: boneName);

            var muzzleFlashTextureOffset = this.description.TextureScreenOffset;
            var boneWorldPosition = this.skeletonRenderer.TransformBonePosition(
                boneName,
                weaponSlotScreenOffset + (Vector2F)muzzleFlashTextureOffset,
                out var worldRotationAngleRad);

            var textureDrawPosition = boneWorldPosition - this.character.Position;

            this.spriteRendererFlash.PositionOffset
                = this.spriteRendererSmoke.PositionOffset
                      = textureDrawPosition;

            this.spriteRendererFlash.RotationAngleRad
                = this.spriteRendererSmoke.RotationAngleRad
                      = worldRotationAngleRad;

            var lightTextureOffset = this.description.LightScreenOffsetRelativeToTexture;
            boneWorldPosition = this.skeletonRenderer.TransformBonePosition(
                boneName,
                weaponSlotScreenOffset
                + (
                      (float)(muzzleFlashTextureOffset.X + lightTextureOffset.X),
                      (float)(muzzleFlashTextureOffset.Y + lightTextureOffset.Y)),
                out _);

            var lightDrawPosition = boneWorldPosition - this.character.Position;
            this.lightSource.PositionOffset = lightDrawPosition;

            this.SetComponentsState(isRendering: true);

            // visualize muzzle flash position
            //ClientComponentPhysicsSpaceVisualizer.ProcessServerDebugPhysicsTesting(
            //    new CircleShape(boneWorldPosition, radius: 0.3, collisionGroup: CollisionGroups.Default));
        }

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
                size: (float)this.description.LightPower);

            this.CreateSpriteRendererAndAnimator(
                out this.spriteRendererSmoke,
                out var componentAnimatorSmoke,
                smokeAtlasRow,
                protoWeapon,
                muzzleFlashTextureAtlas);

            this.CreateSpriteRendererAndAnimator(
                out this.spriteRendererFlash,
                out var componentAnimatorFlash,
                flashAtlasRow,
                protoWeapon,
                muzzleFlashTextureAtlas);

            this.Destroy(this.animationDuration);
            this.lightSource.Destroy(this.animationDuration);
            this.spriteRendererFlash.Destroy(this.animationDuration);
            this.spriteRendererSmoke.Destroy(this.animationDuration);
            componentAnimatorFlash.Destroy(this.animationDuration);
            componentAnimatorSmoke.Destroy(this.animationDuration);

            this.Update(deltaTime: 0);
        }

        public override void Update(double deltaTime)
        {
            this.time += deltaTime;
            if (this.time > this.lightDuration)
            {
                this.lightSource.IsEnabled = false;
                return;
            }

            // calculate animation progress (from 0 to 1, 1 means full recoil animation will be rendered)
            var alpha = (this.lightDuration - this.time) / this.lightDuration;
            this.lightSource.Opacity = alpha;
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

            // to ensure muzzleflash rendering over skeleton renderer we do this offset
            // TODO: find a better way of prioritizing rendering of muzzle flash over skeleton renderer
            spriteRenderer.DrawOrderOffsetY = -0.2;

            // create animator for sprite renderer
            componentAnimatorFlash = this.SceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimatorFlash.Setup(
                spriteRenderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    muzzleFlashTextureAtlas,
                    onlySpecificRow: atlasRow),
                animationFrameDurationSeconds);
        }

        private void SetComponentsState(bool isRendering)
        {
            this.lightSource.IsEnabled = isRendering;
            this.spriteRendererFlash.IsEnabled = isRendering;
            this.spriteRendererSmoke.IsEnabled = isRendering;
        }
    }
}