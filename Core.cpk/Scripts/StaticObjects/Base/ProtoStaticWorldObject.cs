namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.AmbientOcclusion;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    /// <summary>
    /// Base class for static world object type - inherits from base world object type for static world objects.
    /// <br />With specific generics parameters for states.
    /// </summary>
    /// <typeparam name="TPrivateState">Type of server private state.</typeparam>
    /// <typeparam name="TPublicState">Type of server public state.</typeparam>
    /// <typeparam name="TClientState">Type of client state.</typeparam>
    public abstract class ProtoStaticWorldObject
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoWorldObject
          <IStaticWorldObject,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoStaticWorldObject,
          IDamageableProtoWorldObject
        where TPrivateState : BasePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        private TextureResource cachedTextureOcclusion;

        private bool hasCachedTextureOcclusion;

        private ITextureResource icon;

        private IConstructionTileRequirementsReadOnly tileRequirements;

        // every frame
        public override double ClientUpdateIntervalSeconds => 0;

        public virtual ITextureResource DefaultTexture { get; private set; }

        public FinalStatsCache DefenseStats { get; private set; }

        public ITextureResource Icon
        {
            get
            {
                if (this.icon != null)
                {
                    return this.icon;
                }

                Api.ValidateIsClient();

                var createdIcon = this.ClientCreateIcon();
                //if (newIcon is ITextureAtlasResource newIconAsAtlas)
                //{
                // texture atlas! Convert it to the real texture (UI doesn't support atlases)
                var proceduralIcon = new ProceduralTexture(
                    this.ShortId + " icon",
                    isTransparent: true,
                    isUseCache: true,
                    generateTextureCallback: request => ClientObjectIconHelper.CreateIcon(
                                                 createdIcon,
                                                 request),
                    dependsOn: new[] { createdIcon });
                //}

                this.icon = proceduralIcon;
                return this.icon;
            }
        }

        public virtual string InteractionTooltipText => InteractionTooltipTexts.Interact;

        public abstract StaticObjectKind Kind { get; }

        public StaticObjectLayoutReadOnly Layout { get; private set; }

        /// <inheritdoc />
        public abstract double ObstacleBlockDamageCoef { get; }

        public override double ServerUpdateIntervalSeconds => 1;

        public virtual double StructureExplosiveDefenseCoef => 0;

        public abstract float StructurePointsMax { get; }

        public virtual ITextureResource TextureOcclusion
        {
            get
            {
                if (this.hasCachedTextureOcclusion)
                {
                    return this.cachedTextureOcclusion;
                }

                this.hasCachedTextureOcclusion = true;

                var defaultTexture = this.DefaultTexture as TextureResource;
                if (defaultTexture != null)
                {
                    var textureResource = new TextureResource(
                        defaultTexture.LocalPath.Replace(".png", "") + "Occlusion",
                        isTransparent: false);

                    if (Api.Shared.IsFileExists(textureResource))
                    {
                        this.cachedTextureOcclusion = textureResource;
                    }
                }

                return this.cachedTextureOcclusion;
            }
        }

        public IConstructionTileRequirementsReadOnly TileRequirements => this.tileRequirements;

        public BoundsInt ViewBounds { get; private set; }

        /// <summary>
        /// For wide/high objects (and objects with large lights) it's necessary to increase their visual height so they will be
        /// included in view scope properly.
        /// </summary>
        public virtual BoundsInt ViewBoundsExpansion => new BoundsInt(minX: -1, 
                                                                      minY: -1, 
                                                                      maxX: 1, 
                                                                      maxY: 1);

        public bool CheckTileRequirements(Vector2Ushort startTilePosition, ICharacter character, bool logErrors)
        {
            return this.tileRequirements.Check(this, startTilePosition, character, logErrors);
        }

        public virtual string ClientGetTitle(IStaticWorldObject worldObject)
        {
            // only structures are displaying the name tooltip
            return null;
        }

        public void ClientObserving(IStaticWorldObject worldObject, bool isObserving)
        {
            this.ClientObserving(new ClientObjectData(worldObject), isObserving);
        }

        /// <summary>
        /// Gets the texture used for construction place selection (blueprint). Usually the same texture is used for the object
        /// rendering as well.
        /// </summary>
        /// <param name="tile">World tile.</param>
        /// <param name="spriteRenderer"></param>
        /// <param name="positionOffset">Draw offset of texture (return Vector2.Zero if no offset required).</param>
        /// <returns>Texture for blueprint.</returns>
        public virtual void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = this.DefaultTexture;
            this.ClientSetupRenderer(blueprint.SpriteRenderer);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return this.Layout.Center;
        }

        public bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            return this.SharedOnDamage(weaponCache,
                                       (IStaticWorldObject)targetObject,
                                       damagePreMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        public virtual bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            var objectPublicState = GetPublicState(targetObject);
            var previousStructurePoints = objectPublicState.StructurePointsCurrent;
            if (previousStructurePoints <= 0f)
            {
                // already destroyed static world object
                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            var serverDamage = this.SharedCalculateDamageByWeapon(
                weaponCache,
                damagePreMultiplier,
                targetObject,
                out obstacleBlockDamageCoef);

            if (serverDamage < 0)
            {
                Logger.Warning(
                    $"Server damage less than 0 and this is suspicious. {this} calculated damage: {serverDamage:0.###}");
                serverDamage = 0;
            }

            if (IsClient)
            {
                // simply call these methods to display a client notification only!
                // they are not used for anything else here
                // to calculate damage they're used in WeaponDamageSystem.ServerCalculateTotalDamage method.
                RaidingProtectionSystem.SharedCanRaid(targetObject,
                                                      showClientNotification: true);
                PveSystem.SharedIsAllowStructureDamage(weaponCache.Character,
                                                       targetObject,
                                                       showClientNotification: true);

                damageApplied = 0;
                return true;
            }

            // apply damage
            damageApplied = serverDamage;
            var newStructurePoints = (float)(previousStructurePoints - serverDamage);
            if (newStructurePoints < 0)
            {
                newStructurePoints = 0;
            }

            Logger.Info(
                $"Damage applied to {targetObject} by {weaponCache.Character}:\n{serverDamage} dmg, current structure points {newStructurePoints}/{this.StructurePointsMax}, {weaponCache.Weapon}");

            objectPublicState.StructurePointsCurrent = newStructurePoints;

            try
            {
                this.ServerOnStaticObjectDamageApplied(
                    weaponCache,
                    targetObject,
                    previousStructurePoints,
                    newStructurePoints);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Problem on processing {nameof(this.ServerOnStaticObjectDamageApplied)}()");
            }

            if (newStructurePoints <= 0f)
            {
                this.ServerOnStaticObjectZeroStructurePoints(weaponCache, weaponCache.Character, targetObject);
            }

            return true;
        }

        internal static string SharedGetRelativeFolderPath(Type type, Type baseType)
        {
            var ns = type.Namespace;
            var defaultNamespace = baseType.Namespace;

            if (string.Equals(ns, defaultNamespace))
            {
                return string.Empty;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!ns.StartsWith(defaultNamespace))
            {
                throw new Exception(
                    "Namespace for " + type.FullName + " must start with base class namespace: " + defaultNamespace);
            }

            var folderPath = ns.Substring(defaultNamespace.Length + 1).Replace('.', '/');
            return folderPath;
        }

        protected static string GenerateTexturePath(Type type)
        {
            var folderPath = SharedGetRelativeFolderPath(type, typeof(ProtoStaticWorldObject<,,>));
            return $"StaticObjects/{folderPath}/{type.Name}";
        }

        protected void ClientAddAutoStructurePointsBar(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var sceneObject = Client.Scene.GetSceneObject(worldObject);
            sceneObject.AddComponent<ClientComponentAutoDisplayStructurePointsBar>()
                       .Setup(worldObject,
                              structurePointsMax: this.StructurePointsMax);
        }

        protected virtual ITextureResource ClientCreateIcon()
        {
            return this.DefaultTexture;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var clientState = data.ClientState;

            this.ClientAddAutoStructurePointsBar(data);

            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.DefaultTexture);

            this.ClientSetupRenderer(clientState.Renderer);

            var textureOcclusion = this.TextureOcclusion;
            if (textureOcclusion != null)
            {
                clientState.RendererOcclusion = ClientAmbientOcclusion.CreateOcclusionRenderer(
                    worldObject,
                    textureResource: textureOcclusion);

                this.ClientSetupRenderer(clientState.RendererOcclusion);
            }
        }

        protected virtual void ClientObserving(ClientObjectData data, bool isObserving)
        {
        }

        protected override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
        {
            this.MaterialDestroySoundPreset.PlaySound(
                this.ObjectSoundMaterial,
                this,
                worldPosition: tilePosition.ToVector2D() + this.Layout.Center,
                volume: SoundConstants.VolumeDestroy,
                pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        protected virtual void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            if (this.Layout.TileOffsets.Count > 1)
            {
                // draw origin - bottom left corner
                renderer.SpritePivotPoint = (0, 0);
                renderer.PositionOffset = (0, 0);
                return;
            }

            // draw origin - from center/bottom of the current cell
            renderer.SpritePivotPoint = (0.5, 0);
            renderer.PositionOffset = (0.5, 0);
        }

        protected virtual void CreateLayout(StaticObjectLayout layout)
        {
            // default implementation - one-cell object
            layout.Add(0, 0);
        }

        protected string GenerateTexturePath()
        {
            return GenerateTexturePath(this.GetType());
        }

        protected virtual ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource(GenerateTexturePath(thisType));
        }

        protected virtual void PrepareDefense(DefenseDescription defense)
        {
        }

        protected virtual void PrepareProtoStaticWorldObject()
        {
            var tileRequirements = ConstructionTileRequirements.DefaultForStaticObjects.Clone();
            this.PrepareTileRequirements(tileRequirements);
            this.tileRequirements = tileRequirements;
        }

        protected sealed override void PrepareProtoWorldObject()
        {
            base.PrepareProtoWorldObject();

            var defaultTexture = this.PrepareDefaultTexture(this.GetType());
            if (defaultTexture is ITextureAtlasResource defaultTextureAtlas
                && !(defaultTexture is ITextureAtlasChunkResource))
            {
                // use the first chunk of the atlas texture
                defaultTexture = defaultTextureAtlas.Chunk(0, 0);
            }

            this.DefaultTexture = defaultTexture;

            var layout = new StaticObjectLayout();
            this.CreateLayout(layout);
            this.Layout = layout.ToReadOnly();
            this.ViewBounds = this.Layout.CreateViewBounds(this.ViewBoundsExpansion);

            var defenseDescription = new DefenseDescription();

            // Set default defense for static objects. By default there is no defense except for psi and radiation which are set to Max.
            // Individual objects will override this value with some specific tier of defense in their own class.
            defenseDescription.Set(ObjectDefensePresets.Default);

            this.PrepareDefense(defenseDescription);
            var defense = defenseDescription.ToReadOnly();

            using var effects = TempStatsCache.GetFromPool();
            defense.FillEffects(this, effects, maximumDefensePercent: double.MaxValue);
            this.DefenseStats = effects.CalculateFinalStatsCache();

            this.PrepareProtoStaticWorldObject();
        }

        protected virtual void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                data.PublicState.StructurePointsCurrent = this.StructurePointsMax;
            }
        }

        protected virtual void ServerOnStaticObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
        }

        protected virtual void ServerOnStaticObjectDestroyedByCharacter(
            [CanBeNull] ICharacter byCharacter,
            [CanBeNull] IProtoItemWeapon byWeaponProto,
            IStaticWorldObject targetObject)
        {
        }

        protected virtual void ServerOnStaticObjectZeroStructurePoints(
            [CanBeNull] WeaponFinalCache weaponCache,
            [CanBeNull] ICharacter byCharacter,
            [NotNull] IWorldObject targetObject)
        {
            if (targetObject.IsDestroyed)
            {
                return;
            }

            Logger.Info($"Static object destroyed: {targetObject} by {byCharacter}");

            this.ServerSendObjectDestroyedEvent(targetObject);
            Server.World.DestroyObject(targetObject);

            if (weaponCache == null)
            {
                return;
            }

            var staticWorldObject = (IStaticWorldObject)targetObject;
            ServerStaticObjectDestroyObserver.NotifyObjectDestroyed(
                byCharacter,
                staticWorldObject);

            this.ServerOnStaticObjectDestroyedByCharacter(
                byCharacter,
                weaponCache.ProtoWeapon,
                staticWorldObject);
        }

        protected virtual double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            if (IsClient)
            {
                // we don't apply any damage on the Client-side
                return 0;
            }

            if (!weaponCache.ProtoWeapon?.CanDamageStructures ?? false)
            {
                // probably a mob weapon
                obstacleBlockDamageCoef = 1;
                return 0;
            }

            return WeaponDamageSystem.ServerCalculateTotalDamage(
                weaponCache,
                targetObject,
                this.DefenseStats,
                damagePreMultiplier,
                clampDefenseTo1: false);
        }
    }

    /// <summary>
    /// Base class for static world object type - inherits from base world object type for static world objects. Without
    /// specific states.
    /// </summary>
    [NoDoc]
    public abstract class ProtoStaticWorldObject
        : ProtoStaticWorldObject
            <EmptyPrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}