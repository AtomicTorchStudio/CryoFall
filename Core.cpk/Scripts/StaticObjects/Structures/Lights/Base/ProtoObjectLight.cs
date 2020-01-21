namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLight
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectLight,
          IInteractableProtoWorldObject
        where TPrivateState : ObjectLightPrivateState, new()
        where TPublicState : ObjectLightPublicState, new()
        where TClientState : ObjectLightClientState, new()
    {
        /// <summary>
        /// Minimal distance to a character to turn the light on-off automatically
        /// </summary>
        private const int AutoLightDistance = 11;

        private TextureAtlasResource textureAtlas;

        public virtual byte ContainerInputSlotsCount => 1;

        public double FuelAmountInitial { get; private set; }

        public double FuelCapacity { get; private set; }

        public IFuelItemsContainer FuelItemsContainerPrototype { get; private set; }

        public double FuelUsePerSecond { get; private set; }

        public override bool HasIncreasedScopeSize => true;

        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public abstract Color LightColor { get; }

        public abstract double LightSize { get; }

        public virtual Vector2D LightWorldOffset => (0.5, 0.5);

        protected virtual Vector2D LightWorldSpritePivotPoint => (0.5, 0.5);

        public void ClientSetLightMode(IStaticWorldObject lightObject, ObjectLightMode mode)
        {
            this.CallServer(_ => _.ServerRemote_SetLightMode(lightObject, mode));
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

            // not the land owner
            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - not the land owner.",
                    character);

                if (IsServer)
                {
                    LandClaimSystem.ServerNotifyCannotInteractNotOwner(character, worldObject);
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

        protected override ITextureResource ClientCreateIcon()
            => this.textureAtlas is null
                   ? this.DefaultTexture
                   : this.textureAtlas.Chunk(1, 0);

        protected virtual BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: this.LightColor,
                size: (float)this.LightSize,
                spritePivotPoint: this.LightWorldSpritePivotPoint,
                positionOffset: this.LightWorldOffset);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            data.ClientState.RendererLight = this.ClientCreateLightSource(worldObject.ClientSceneObject);

            // subscribe to active state changes
            publicState.ClientSubscribe(
                _ => _.IsLightActive,
                isActive => { this.ClientIsActiveChanged(data); },
                data.ClientState);

            this.ClientIsActiveChanged(data);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual void ClientIsActiveChanged(ClientInitializeData data)
        {
            var isActive = data.PublicState.IsLightActive;

            var clientState = data.ClientState;
            clientState.Renderer.TextureResource = this.textureAtlas.Chunk((byte)(isActive ? 1 : 0), 0);
            clientState.RendererLight.IsEnabled = isActive;
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowObjectLight.Open(
                new ViewModelWindowObjectLight(
                    data.GameObject,
                    data.PrivateState,
                    data.PublicState));
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.textureAtlas = new TextureAtlasResource(
                GenerateTexturePath(thisType),
                columns: 2,
                rows: 1,
                isTransparent: true);

            return this.textureAtlas;
        }

        protected abstract void PrepareFuelConfig(
            out double fuelCapacity,
            out double fuelAmountInitial,
            out double fuelUsePerSecond,
            out IFuelItemsContainer fuelContainerPrototype);

        protected virtual void PrepareProtoObjectLight()
        {
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.PrepareFuelConfig(
                out var fuelCapacity,
                out var fuelAmountInitial,
                out var fuelUsePerSecond,
                out var fuelContainerPrototype);

            this.FuelCapacity = fuelCapacity;
            this.FuelAmountInitial = fuelAmountInitial;
            if (fuelAmountInitial > fuelCapacity)
            {
                throw new Exception(
                    $"{nameof(this.FuelAmountInitial)} ({fuelAmountInitial:0.##}) is bigger than {nameof(this.FuelCapacity)} ({fuelCapacity:0.##})");
            }

            this.FuelUsePerSecond = fuelUsePerSecond;

            this.FuelItemsContainerPrototype = fuelContainerPrototype
                                               ?? throw new Exception(
                                                   "Fuel list is empty, check " + nameof(this.PrepareFuelConfig));

            this.PrepareProtoObjectLight();
        }

        protected virtual bool ServerCheckCanLight(IStaticWorldObject lightObject, double fuelAmount)
        {
            return fuelAmount > 0;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;

            if (data.IsFirstTimeInit)
            {
                privateState.FuelAmount = this.FuelAmountInitial;
                privateState.Mode = ObjectLightMode.Auto;
            }

            var containerInput = privateState.ContainerInput;
            if (containerInput == null)
            {
                containerInput =
                    Server.Items.CreateContainer(data.GameObject, this.ContainerInputSlotsCount);
                privateState.ContainerInput = containerInput;
            }
            else
            {
                Server.Items.SetSlotsCount(containerInput, this.ContainerInputSlotsCount);
            }

            Server.Items.SetContainerType(containerInput, this.FuelItemsContainerPrototype);
        }

        protected virtual void ServerTryConsumeFuelItem(TPrivateState privateState)
        {
            var fuelAmount = privateState.FuelAmount;

            // try consume input item and add it's organic value into the mulchbox organic amount
            var inputItem = privateState.ContainerInput.Items.FirstOrDefault();
            if (inputItem != null
                && inputItem.ProtoItem is IProtoItemFuel protoItemFuel)
            {
                var count = inputItem.Count;
                while (count > 0)
                {
                    if (fuelAmount > 0
                        && fuelAmount + protoItemFuel.FuelAmount > this.FuelCapacity)
                    {
                        // don't add this item - it will lead to over capacity
                        break;
                    }

                    fuelAmount += protoItemFuel.FuelAmount;
                    count--;
                }

                fuelAmount = Math.Min(fuelAmount, this.FuelCapacity);

                privateState.FuelAmount = fuelAmount;

                if (count != inputItem.Count)
                {
                    Server.Items.SetCount(inputItem, count);
                }
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            this.ServerUpdateLight(data.GameObject,
                                   data.PrivateState,
                                   data.PublicState,
                                   data.DeltaTime);
        }

        private bool ServerIsLightShouldBeActive(
            IStaticWorldObject lightObject,
            double fuelAmount,
            ObjectLightMode mode)
        {
            if (!this.ServerCheckCanLight(lightObject, fuelAmount))
            {
                return false;
            }

            if (mode == ObjectLightMode.On)
            {
                return true;
            }

            if (mode == ObjectLightMode.Auto
                && TimeOfDaySystem.IsNight)
            {
                using var charactersNearby = Api.Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(lightObject, charactersNearby);
                if (charactersNearby.Count == 0)
                {
                    return false;
                }

                foreach (var character in charactersNearby.AsList())
                {
                    if (character.ProtoCharacter is PlayerCharacterSpectator)
                    {
                        continue;
                    }

                    if (character.TilePosition.TileSqrDistanceTo(lightObject.TilePosition)
                        < AutoLightDistance * AutoLightDistance)
                    {
                        // close enough to turn the light automatically
                        return true;
                    }
                }
            }

            return false;
        }

        private void ServerRemote_SetLightMode(IStaticWorldObject lightObject, ObjectLightMode mode)
        {
            var character = ServerRemoteContext.Character;
            if (!InteractionCheckerSystem.SharedHasInteraction(character, lightObject, requirePrivateScope: true))
            {
                throw new Exception("The player character is not interacting with the light object");
            }

            var privateState = GetPrivateState(lightObject);
            var publicState = GetPublicState(lightObject);

            privateState.Mode = mode;
            Logger.Important($"Light mode changed: {mode}, light: {lightObject}", character);

            this.ServerUpdateLight(lightObject,
                                   privateState,
                                   publicState,
                                   deltaTime: 0);
        }

        private void ServerUpdateLight(
            IStaticWorldObject lightObject,
            TPrivateState privateState,
            TPublicState publicState,
            double deltaTime)
        {
            this.ServerTryConsumeFuelItem(privateState);

            var fuelAmount = privateState.FuelAmount;

            // active if has fuel and mode allows
            var mode = privateState.Mode;
            var isActive = this.ServerIsLightShouldBeActive(lightObject, fuelAmount, mode);
            publicState.IsLightActive = isActive;

            if (!isActive)
            {
                publicState.IsLightActive = false;
                return;
            }

            // burn fuel
            fuelAmount -= this.FuelUsePerSecond * deltaTime;
            if (fuelAmount < 0)
            {
                fuelAmount = 0;
            }

            privateState.FuelAmount = fuelAmount;
        }
    }

    public abstract class ProtoObjectLight
        : ProtoObjectLight<ObjectLightPrivateState, ObjectLightPublicState, ObjectLightClientState>
    {
    }
}