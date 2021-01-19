namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Characters.Turrets;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Turrets;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public abstract class ProtoObjectTurret
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectTurret,
          IInteractableProtoWorldObject,
          IProtoObjectElectricityConsumerWithCustomRate
        where TPrivateState : ObjectTurretPrivateState, new()
        where TPublicState : ObjectTurretPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string ErrorTooCloseToAnotherTurret = "Too close to another turret";

        private const int MinDistanceBetweenTurrets = 2;

        public static readonly Lazy<SolidColorBrush> AttackRangeBlueprintBorderBrush
            = new(() => new SolidColorBrush(Color.FromArgb(0x99, 0xEE, 0x44, 0x00)));

        private static readonly Lazy<SolidColorBrush> AttackRangeBlueprintFillBrush
            = new(() => new SolidColorBrush(Color.FromArgb(0x22, 0xEE, 0x44, 0x00)));

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToAnotherTurret
            = new(ErrorTooCloseToAnotherTurret,
                  c =>
                  {
                      var startPosition = c.StartTilePosition;
                      var objectsInBounds = SharedFindObjectsNearby<IProtoObjectStructure>(startPosition);
                      foreach (var obj in objectsInBounds)
                      {
                          if (ReferenceEquals(obj, c.ObjectToRelocate))
                          {
                              continue;
                          }

                          if (obj.ProtoGameObject is IProtoObjectTurret)
                          {
                              // found another turret nearby
                              return false;
                          }
                      }

                      return true;
                  });

        private IProtoCharacterTurret protoCharacterTurret;

        [CanBeNull]
        public abstract BaseItemsContainerTurretAmmo ContainerAmmoType { get; }

        public ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 1, shutdownPercent: 0);

        public virtual float DestroyedExplosionVolume => 1;

        public DamageDescription DestroyedTurretDamageDescriptionCharacters { get; private set; }

        public double DestroyedTurretDamageRadius { get; private set; }

        public abstract double ElectricityConsumptionPerSecondWhenActive { get; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => false;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double ServerUpdateIntervalSeconds => 0.25;

        protected virtual byte DestroyedExplosionNetworkRadius => 30;

        protected ExplosionPreset DestroyedTurretExplosionPreset { get; private set; }

        protected virtual ITextureResource TextureResourceBottom { get; private set; }

        public void ClientSetTurretMode(IStaticWorldObject objectTurret, TurretMode mode)
        {
            this.CallServer(_ => _.ServerRemote_SetTurretMode(objectTurret, mode));
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            // setup sprite renderer for the turret skeleton 
            var renderer = blueprint.SpriteRenderer;
            renderer.TextureResource = this.protoCharacterTurret.Icon;
            renderer.Size = (ScriptingConstants.TileSizeRealPixels,
                             ScriptingConstants.TileSizeRealPixels);
            renderer.PositionOffset = (-0.5, 0.5);
            renderer.SpritePivotPoint = (0, 0.5);

            this.protoCharacterTurret.SharedGetSkeletonProto(null, out var skeleton, out _);
            renderer.PositionOffset += (0, ((ProtoSkeletonTurret)skeleton).DrawWorldPositionOffsetY);

            if (blueprint.IsConstructionSite)
            {
                return;
            }

            // setup circle renderer for the turret attack range
            var bounds = this.Layout.Bounds;
            var protoItemWeaponTurret = this.protoCharacterTurret.ProtoItemWeaponTurret;
            var weaponRangeMax = protoItemWeaponTurret.OverrideDamageDescription?.RangeMax
                                 ?? (protoItemWeaponTurret.CompatibleAmmoProtos.Max(w => w.DamageDescription.RangeMax)
                                     * protoItemWeaponTurret.RangeMultiplier);
            var sizeX = bounds.MaxX - bounds.MinX + 2 * weaponRangeMax - 1;
            var sizeY = bounds.MaxY - bounds.MinY + 2 * weaponRangeMax - 1;
            var ellipse = new Ellipse()
            {
                Width = sizeX * ScriptingConstants.TileSizeVirtualPixels,
                Height = sizeY * ScriptingConstants.TileSizeVirtualPixels,
                Fill = AttackRangeBlueprintFillBrush.Value,
                Stroke = AttackRangeBlueprintBorderBrush.Value,
                StrokeThickness = 4
            };

            // workaround for NoesisGUI
            ellipse.SetValue(Shape.StrokeDashArrayProperty, "5,2.5");

            Api.Client.UI.AttachControl(
                blueprint.SceneObject,
                positionOffset: this.Layout.Center,
                uiElement: ellipse,
                isFocusable: false,
                isScaleWithCameraZoom: true);
        }

        public void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            var turret = GetPrivateState(gameObject).ServerCharacterTurret;
            Server.World.DestroyObject(turret);

            base.ServerOnDestroy(gameObject);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                GetPrivateState(gameObject).ContainerAmmo,
                destroyTimeout: TimeSpan.FromMinutes(60).TotalSeconds);
        }

        public void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return base.SharedCanInteract(character, worldObject, writeToLog)
                   && (CreativeModeSystem.SharedIsInCreativeMode(character)
                       || LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject,
                                                                              character,
                                                                              requireFactionPermission: true,
                                                                              out _));
        }

        public double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            return LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(worldObject)
                       ? 0
                       : 1;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
            IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        // Please note: the start position is located in bottom left corner of the layout.
        protected static IEnumerable<IStaticWorldObject> SharedFindObjectsNearby
            <TProtoObject>(Vector2Ushort startPosition)
            where TProtoObject : class, IProtoStaticWorldObject
        {
            var world = IsServer
                            ? (IWorldService)Server.World
                            : (IWorldService)Client.World;

            var bounds = new RectangleInt(startPosition, size: (1, 1));
            bounds = bounds.Inflate(MinDistanceBetweenTurrets);

            var objectsInBounds = world.GetStaticWorldObjectsOfProtoInBounds<TProtoObject>(bounds);
            return objectsInBounds;
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return this.protoCharacterTurret.Icon;
        }

        protected sealed override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // preload all the explosion spritesheets
            foreach (var textureAtlasResource in this.DestroyedTurretExplosionPreset.SpriteAtlasResources)
            {
                Client.Rendering.PreloadTextureAsync(textureAtlasResource);
            }

            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
            TurretNoAmmoOverlay.CreateFor(data.GameObject);
            this.ClientInitializeTurret(data);
        }

        protected virtual void ClientInitializeTurret(ClientInitializeData data)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowTurret.Open(
                new ViewModelWindowTurret(data.GameObject));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.IsEnabled = false;
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            tileRequirements.Add(LandClaimSystem.ValidatorIsOwnedLand);
            tileRequirements.Add(ValidatorTooCloseToAnotherTurret);

            category = GetCategory<StructureCategoryElectricity>();

            this.PrepareConstructionConfigTurret(tileRequirements,
                                                 build,
                                                 repair,
                                                 upgrade);
        }

        protected abstract void PrepareConstructionConfigTurret(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = GenerateTexturePath(thisType);
            this.TextureResourceBottom = new TextureResource(path + "Bottom");
            return this.TextureResourceBottom;
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();
            this.PrepareProtoTurretObject(out this.protoCharacterTurret);
            if (this.protoCharacterTurret is null)
            {
                throw new Exception("Prototype for turret character is not provided");
            }

            this.PrepareProtoTurretDestroyedExplosionPreset(
                out var destroyedTurretDamageRadius,
                out var destroyedTurretExplosionPreset,
                out var destroyedTurretDamageDescriptionCharacters);

            this.DestroyedTurretExplosionPreset = destroyedTurretExplosionPreset
                                                  ?? throw new Exception("No explosion preset provided");

            this.DestroyedTurretDamageRadius = destroyedTurretDamageRadius;
            this.DestroyedTurretDamageDescriptionCharacters = destroyedTurretDamageDescriptionCharacters;
        }

        protected virtual void PrepareProtoTurretDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters)
        {
            damageRadius = 2.1;
            explosionPreset = ExplosionPresets.Large;

            damageDescriptionCharacters = new DamageDescription(
                damageValue: 75,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: damageRadius,
                damageDistribution: new DamageDistribution(DamageType.Kinetic, 1));
        }

        protected abstract void PrepareProtoTurretObject(out IProtoCharacterTurret protoCharacter);

        protected virtual void ServerExecuteTurretExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DestroyedTurretDamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: false,
                isDamageThroughObstacles: false,
                callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance);

            double CalcDamageCoefByDistance(double distance)
            {
                var distanceThreshold = 0.5;
                if (distance <= distanceThreshold)
                {
                    return 1;
                }

                distance -= distanceThreshold;
                distance = Math.Max(0, distance);

                var maxDistance = this.DestroyedTurretDamageRadius;
                maxDistance -= distanceThreshold;
                maxDistance = Math.Max(0, maxDistance);

                return 1 - Math.Min(distance / maxDistance, 1);
            }
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;

            if (data.IsFirstTimeInit)
            {
                privateState.TurretMode = TurretMode.AttackHostile;
            }

            var characterTurret = privateState.ServerCharacterTurret;
            if (characterTurret is null
                || characterTurret.IsDestroyed)
            {
                characterTurret = Server.Characters.SpawnCharacter(this.protoCharacterTurret,
                                                                   worldObject.TilePosition.ToVector2D() + (0.5, 0));
                privateState.ServerCharacterTurret = characterTurret;
            }

            // setup optional ammo items container
            var containerAmmo = privateState.ContainerAmmo;
            var containerAmmoType = this.ContainerAmmoType;
            if (containerAmmoType is null)
            {
                if (containerAmmo is not null)
                {
                    Server.Items.DestroyContainer(containerAmmo);
                    privateState.ContainerAmmo = null;
                    containerAmmo = null;
                }
            }
            else
            {
                var containerAmmoSlotsCount = containerAmmoType.SlotsCount;
                if (containerAmmo is not null)
                {
                    // container already created - update it
                    Server.Items.SetContainerType(containerAmmo, containerAmmoType);
                    Server.Items.SetSlotsCount(containerAmmo,
                                               slotsCount: containerAmmoSlotsCount);
                }
                else
                {
                    containerAmmo = Server.Items.CreateContainer(
                        owner: worldObject,
                        itemsContainerType: containerAmmoType,
                        slotsCount: containerAmmoSlotsCount);

                    privateState.ContainerAmmo = containerAmmo;
                }
            }

            var characterTurretPrivateState = characterTurret
                .GetPrivateState<CharacterTurretPrivateState>();
            characterTurretPrivateState.ObjectTurret = worldObject;
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            if (byCharacter?.SharedGetPlayerSelectedHotbarItemProto() is ProtoItemToolCrowbar)
            {
                // deconstructed with a crowbar
                base.ServerOnStaticObjectZeroStructurePoints(weaponCache,
                                                             byCharacter,
                                                             targetObject);
                return;
            }

            // destroyed - explode
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetCharactersInRadius(targetObject.TilePosition,
                                               scopedBy,
                                               radius: this.DestroyedExplosionNetworkRadius,
                                               onlyPlayers: true);

            this.CallClient(scopedBy.AsList(),
                            _ => _.ClientRemote_TurretExploded(targetObject.TilePosition));

            SharedExplosionHelper.ServerExplode(
                character:
                null, // yes, no damaging character here otherwise it will not receive the damage if staying close
                protoExplosive: null,
                protoWeapon: null,
                explosionPreset: this.DestroyedTurretExplosionPreset,
                epicenterPosition: targetObject.TilePosition.ToVector2D()
                                   + this.SharedGetObjectCenterWorldOffset(targetObject),
                damageDescriptionCharacters: this.DestroyedTurretDamageDescriptionCharacters,
                physicsSpace: targetObject.PhysicsBody.PhysicsSpace,
                executeExplosionCallback: this.ServerExecuteTurretExplosion);

            // destroy the turret completely after short explosion delay
            ServerTimersSystem.AddAction(
                this.DestroyedTurretExplosionPreset.ServerDamageApplyDelay,
                () => base.ServerOnStaticObjectZeroStructurePoints(weaponCache,
                                                                   byCharacter,
                                                                   targetObject));
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            if (privateState.ContainerAmmo is null)
            {
                // this turret doesn't have ammo container / no ammo usage
                return;
            }

            var currentAmmoItem = privateState.ContainerAmmo.Items.LastOrDefault();
            var characterTurret = privateState.ServerCharacterTurret;
            var characterTurretPrivateState = characterTurret.GetPrivateState<CharacterTurretPrivateState>();
            characterTurretPrivateState.CurrentAmmoItem = currentAmmoItem;
            data.PublicState.HasNoAmmo = currentAmmoItem is null;
        }

        private void ClientRemote_TurretExploded(Vector2Ushort tilePosition)
        {
            Logger.Important(this + " exploded at " + tilePosition);
            SharedExplosionHelper.ClientExplode(position: tilePosition.ToVector2D()
                                                          + this.SharedGetObjectCenterWorldOffset(null),
                                                this.DestroyedTurretExplosionPreset,
                                                this.DestroyedExplosionVolume);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.5, keyArgIndex: 0)]
        private void ServerRemote_SetTurretMode(IStaticWorldObject objectTurret, TurretMode mode)
        {
            this.VerifyGameObject(objectTurret);

            var character = ServerRemoteContext.Character;
            if (!objectTurret.ProtoStaticWorldObject.SharedCanInteract(character,
                                                                       objectTurret,
                                                                       writeToLog: true))
            {
                return;
            }

            var privateState = GetPrivateState(objectTurret);
            if (privateState.TurretMode == mode)
            {
                return;
            }

            privateState.TurretMode = mode;
            Logger.Info($"Changed turret mode for {objectTurret}: {mode}");
        }
    }

    public abstract class ProtoObjectTurret
        : ProtoObjectTurret
            <ObjectTurretPrivateState,
                ObjectTurretPublicState,
                StaticObjectClientState>
    {
    }
}