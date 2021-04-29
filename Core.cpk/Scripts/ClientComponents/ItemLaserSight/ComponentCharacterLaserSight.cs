namespace AtomicTorch.CBND.CoreMod.ClientComponents.ItemLaserSight
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    internal class ComponentCharacterLaserSight : ClientComponent
    {
        private const double BeamWidth = 0.1;

        private static readonly Color RayColorCurrentPlayerCharacter
            = Color.FromArgb(0x44, 0x00, 0xFF, 0x00);

        private static readonly Color RayColorFriendlyPlayerCharacter
            = Color.FromArgb(0x30, 0x00, 0xFF, 0x00);

        private static readonly Color RayColorOtherPlayerCharacter
            = Color.FromArgb(0x44, 0xFF, 0x00, 0x00);

        private ComponentBeam componentBeam;

        private ICharacter currentCharacter;

        private ItemLaserSight.PublicState itemPublicState;

        public ComponentCharacterLaserSight() : base(isLateUpdateEnabled: true)
        {
        }

        [CanBeNull]
        public static ComponentCharacterLaserSight TryCreateComponent(
            ICharacter character,
            ItemLaserSight.PublicState itemPublicState)
        {
            if (!(character.ProtoGameObject is PlayerCharacter))
            {
                return null;
            }

            var sceneObject = character.ClientSceneObject;
            var component = sceneObject.AddComponent<ComponentCharacterLaserSight>();
            component.currentCharacter = character;
            component.itemPublicState = itemPublicState;
            return component;
        }

        public override void LateUpdate(double deltaTime)
        {
            var characterPublicState = this.currentCharacter.GetPublicState<ICharacterPublicState>();
            if (characterPublicState.IsDead
                || (characterPublicState is PlayerCharacterPublicState playerCharacterPublicState
                    && !playerCharacterPublicState.IsOnline)
                || !(characterPublicState.SelectedItemWeaponProto is IProtoItemWeaponRanged protoWeaponRanged))
            {
                this.componentBeam.IsEnabled = false;
                return;
            }

            var rangeMax = this.currentCharacter.IsCurrentClientCharacter
                               ? ItemLaserSight.SharedGetCurrentRangeMax(characterPublicState)
                               : this.itemPublicState.MaxRange;
            if (rangeMax <= 1.5)
            {
                // no weapon selected or a too close-range weapon
                this.componentBeam.IsEnabled = false;
                return;
            }

            var clientState = this.currentCharacter.GetClientState<BaseCharacterClientState>();
            if (clientState.SkeletonRenderer is null
                || !clientState.SkeletonRenderer.IsReady)
            {
                this.componentBeam.IsEnabled = false;
                return;
            }

            var customTargetPosition = this.currentCharacter.IsCurrentClientCharacter
                                       && protoWeaponRanged is IProtoItemWeaponGrenadeLauncher
                                           ? (Vector2D?)WeaponGrenadeLauncherHelper.ClientGetCustomTargetPosition()
                                           : default;

            CastLine(this.currentCharacter,
                     customTargetPosition: customTargetPosition,
                     rangeMax: rangeMax,
                     protoWeaponRanged,
                     collisionGroup: protoWeaponRanged.CollisionGroup,
                     toPosition: out var toPosition,
                     hitPosition: out var hitPosition);

            var sourcePosition = this.GetSourcePosition(protoWeaponRanged);
            var beamEndPosition = hitPosition ?? toPosition;

            // fade-out if too close to prevent visual glitches
            var distance = (beamEndPosition - sourcePosition).Length;
            var beamOpacity = MathHelper.Clamp(distance - 0.5, min: 0, max: 1);
            beamOpacity = beamOpacity * beamOpacity * beamOpacity;

            var color = this.GetColor();
            this.componentBeam.IsEnabled = true;
            this.componentBeam.Refresh(
                sourcePosition: sourcePosition,
                sourcePositionOffset: 0.1,
                targetPosition: beamEndPosition,
                beamWidth: BeamWidth,
                beamColor: color,
                spotColor: color,
                beamOpacity: beamOpacity,
                // determine whether the beam should end with a bright spot (when pointing on something)
                hasTarget: hitPosition.HasValue
                           || protoWeaponRanged is IProtoItemWeaponGrenadeLauncher);
        }

        protected override void OnDisable()
        {
            this.componentBeam.Destroy();
            this.componentBeam = null;
        }

        protected override void OnEnable()
        {
            this.componentBeam = this.SceneObject.AddComponent<ComponentBeam>(isEnabled: false);
        }

        private static void CastLine(
            ICharacter character,
            Vector2D? customTargetPosition,
            double rangeMax,
            IProtoItemWeaponRanged protoWeaponRanged,
            CollisionGroup collisionGroup,
            out Vector2D toPosition,
            out Vector2D? hitPosition)
        {
            hitPosition = null;

            var clientState = character.GetClientState<BaseCharacterClientState>();
            var characterRotationAngleRad = clientState.LastInterpolatedRotationAngleRad.HasValue
                                                ? clientState.LastInterpolatedRotationAngleRad.Value
                                                : ((IProtoCharacterCore)character.ProtoCharacter)
                                                .SharedGetRotationAngleRad(character);

            WeaponSystem.SharedCastLine(character,
                                        isMeleeWeapon: false,
                                        rangeMax,
                                        characterRotationAngleRad,
                                        customTargetPosition: customTargetPosition,
                                        fireSpreadAngleOffsetDeg: 0,
                                        collisionGroup: collisionGroup,
                                        toPosition: out toPosition,
                                        tempLineTestResults: out var tempLineTestResults,
                                        sendDebugEvent: false);

            var characterCurrentVehicle = character.SharedGetCurrentVehicle();
            var characterTileHeight = character.Tile.Height;

            using (tempLineTestResults)
            {
                foreach (var testResult in tempLineTestResults.AsList())
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;

                    var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
                    if (attackedProtoTile is not null)
                    {
                        if (attackedProtoTile.Kind != TileKind.Solid)
                        {
                            // non-solid obstacle - skip
                            continue;
                        }

                        var attackedTile = Client.World.GetTile((Vector2Ushort)testResultPhysicsBody.Position);
                        if (attackedTile.Height < characterTileHeight)
                        {
                            // attacked tile is below - ignore it
                            continue;
                        }

                        // tile on the way - blocking damage ray
                        hitPosition = testResultPhysicsBody.Position
                                      + testResult.Penetration;
                        break;
                    }

                    var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
                    if (ReferenceEquals(damagedObject,    character)
                        || ReferenceEquals(damagedObject, characterCurrentVehicle))
                    {
                        // ignore collision with self
                        continue;
                    }

                    if (damagedObject.ProtoGameObject is not IDamageableProtoWorldObject protoDamageableWorldObject)
                    {
                        // shoot through this object
                        continue;
                    }

                    if (!protoDamageableWorldObject.SharedIsObstacle(damagedObject, protoWeaponRanged))
                    {
                        // can shoot through this object
                        continue;
                    }

                    // don't allow damage is there is no direct line of sight on physical colliders layer between the two objects
                    if (WeaponSystem.SharedHasTileObstacle(character.Position,
                                                           characterTileHeight,
                                                           damagedObject,
                                                           targetPosition: testResult.PhysicsBody.Position
                                                                           + testResult.PhysicsBody.CenterOffset))
                    {
                        continue;
                    }

                    hitPosition = testResultPhysicsBody.Position
                                  + testResult.Penetration;
                    /*+ WeaponSystem.SharedOffsetHitWorldPositionCloserToObjectCenter(
                        damagedObject,
                        damagedObject.ProtoWorldObject,
                        hitPoint: testResult.Penetration,
                        isRangedWeapon: true);*/
                    break;
                }
            }
        }

        private Color GetColor()
        {
            if (this.currentCharacter.IsCurrentClientCharacter)
            {
                return RayColorCurrentPlayerCharacter;
            }

            return PartySystem.ClientIsPartyMember(this.currentCharacter.Name)
                   || FactionSystem.ClientIsFactionMemberOrAlly(this.currentCharacter)
                       ? RayColorFriendlyPlayerCharacter
                       : RayColorOtherPlayerCharacter;
        }

        private Vector2D GetSourcePosition(IProtoItemWeaponRanged protoWeapon)
        {
            // calculate sprite position offset
            var clientState = this.currentCharacter.GetClientState<BaseCharacterClientState>();
            var skeletonRenderer = clientState.SkeletonRenderer;
            var slotName = clientState.CurrentProtoSkeleton.SlotNameItemInHand;
            var weaponSlotScreenOffset = skeletonRenderer.GetSlotScreenOffset(attachmentName: slotName);
            var muzzleFlashTextureOffset = protoWeapon.MuzzleFlashDescription.TextureScreenOffset;
            return skeletonRenderer.TransformSlotPosition(
                slotName,
                weaponSlotScreenOffset + (Vector2F)muzzleFlashTextureOffset,
                out _);
        }

        private class ComponentBeam : ClientComponent
        {
            private const double SpotScale = 4.0;

            private static readonly EffectResource BeamEffectResource
                = new("AdditiveColorEffect");

            private static readonly TextureResource TextureResourceBeam
                = new("FX/WeaponTraces/BeamLaser.png");

            private static readonly TextureResource TextureResourceSpot
                = new("FX/Special/LaserSightSpot.png");

            private readonly RenderingMaterial renderingMaterial
                = RenderingMaterial.Create(BeamEffectResource);

            private IComponentSpriteRenderer spriteRendererLine;

            private IComponentSpriteRenderer spriteRendererSpot;

            public void Refresh(
                Vector2D sourcePosition,
                double sourcePositionOffset,
                Vector2D targetPosition,
                double beamWidth,
                Color beamColor,
                Color spotColor,
                double beamOpacity,
                bool hasTarget)
            {
                beamColor = Color.FromArgb((byte)(beamOpacity * beamColor.A),
                                           beamColor.R,
                                           beamColor.G,
                                           beamColor.B);

                this.renderingMaterial.EffectParameters.Set("ColorAdditive", beamColor);
                this.spriteRendererLine.Color = beamColor;
                this.spriteRendererSpot.Color = spotColor;

                var deltaPos = targetPosition - sourcePosition;

                ComponentWeaponTrace.CalculateAngleAndDirection(deltaPos,
                                                                out var angleRad,
                                                                out var normalizedRay);
                sourcePosition += normalizedRay * sourcePositionOffset;
                deltaPos = targetPosition - sourcePosition;

                var sceneObjectPosition = this.spriteRendererLine.SceneObject.Position;
                this.spriteRendererLine.PositionOffset = sourcePosition - sceneObjectPosition;
                this.spriteRendererLine.RotationAngleRad = (float)angleRad;
                this.spriteRendererLine.Size = (ScriptingConstants.TileSizeVirtualPixels * deltaPos.Length,
                                                ScriptingConstants.TileSizeVirtualPixels * beamWidth);

                this.spriteRendererSpot.PositionOffset = targetPosition - sceneObjectPosition;
                this.spriteRendererSpot.Size = beamWidth * SpotScale * ScriptingConstants.TileSizeVirtualPixels;
                this.spriteRendererSpot.IsEnabled = hasTarget;
            }

            protected override void OnDisable()
            {
                this.spriteRendererLine.Destroy();
                this.spriteRendererSpot.Destroy();
            }

            protected override void OnEnable()
            {
                var sceneObject = this.SceneObject;
                this.spriteRendererLine = Api.Client.Rendering.CreateSpriteRenderer(
                    sceneObject,
                    textureResource: TextureResourceBeam,
                    spritePivotPoint: (0, 0.5),
                    drawOrder: DrawOrder.Light);
                this.spriteRendererLine.RenderingMaterial = this.renderingMaterial;
                this.spriteRendererLine.BlendMode = BlendMode.AdditivePremultiplied;

                this.spriteRendererSpot = Api.Client.Rendering.CreateSpriteRenderer(
                    sceneObject,
                    textureResource: TextureResourceSpot,
                    spritePivotPoint: (0.5, 0.5),
                    drawOrder: DrawOrder.Light);
                this.spriteRendererSpot.RenderingMaterial = this.renderingMaterial;
                this.spriteRendererSpot.BlendMode = BlendMode.AdditivePremultiplied;
            }
        }
    }
}