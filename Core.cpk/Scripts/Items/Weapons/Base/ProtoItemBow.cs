namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemBow
        : ProtoItemWeaponRanged
            <WeaponPrivateState, WeaponBowPublicState, EmptyClientState>
    {
        private static readonly Dictionary<IProtoItemAmmo, IReadOnlyDropItemsList> ServerCachedDroplists
            = new();

        public abstract double TimeToReadyAfterReloading { get; }

        protected TextureResource CachedWeaponTextureResourceReady { get; private set; }

        protected virtual TextureResource WeaponReadyTextureResource
        {
            get
            {
                var path = this.WeaponTextureResource.LocalPath;
                path = path.Substring(0, path.Length - 4); // remove .png
                return new(path + "Ready.png",
                           isProvidesMagentaPixelPosition: true);
            }
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview = false)
        {
            base.ClientSetupSkeleton(item,
                                     character,
                                     protoCharacterSkeleton,
                                     skeletonRenderer,
                                     skeletonComponents,
                                     isPreview);
            if (item is not null)
            {
                this.ClientRefreshWeaponInHandSprite(item);
            }
        }

        public override void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop)
        {
            if (damagedObject is ICharacter)
            {
                base.SharedOnHit(weaponCache, damagedObject, damage, hitData, out isDamageStop);
            }

            // make arrows to always stop on the first hit and have a chance to drop on the ground
            isDamageStop = true;

            if (IsServer
                && isDamageStop
                && RandomHelper.NextDouble() <= 2 / 3.0) // 2/3 chance to recover the arrow
            {
                this.ServerCreateDroppedArrow(weaponCache,
                                              hitData.FallbackTilePosition.ToVector2D() + hitData.HitPoint);
            }
        }

        public override void SharedOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            base.SharedOnMiss(weaponCache, endPosition);

            if (IsServer) // 100% chance to recover the arrow
            {
                this.ServerCreateDroppedArrow(weaponCache, endPosition);
            }
        }

        public override void SharedOnWeaponAmmoChanged(IItem item, ushort ammoCount)
        {
            base.SharedOnWeaponAmmoChanged(item, ammoCount);

            var publicState = GetPublicState(item);
            var isReady = ammoCount > 0;
            publicState.IsReady = isReady;

            if (!isReady)
            {
                return;
            }

            var owner = item.Container.OwnerAsCharacter;
            if (IsClient
                && owner != ClientCurrentCharacterHelper.Character)
            {
                return;
            }

            var weaponState = PlayerCharacter.GetPrivateState(owner).WeaponState;
            weaponState.CooldownSecondsRemains +=
                Api.Shared.RoundDurationByServerFrameDuration(this.TimeToReadyAfterReloading);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            Client.Rendering.PreloadTextureAsync(this.CachedWeaponTextureResourceReady);

            var item = data.GameObject;
            var publicState = data.PublicState;

            publicState.ClientSubscribe(
                _ => _.IsReady,
                isReady => this.ClientRefreshWeaponInHandSprite(item),
                data.ClientState);

            this.ClientRefreshWeaponInHandSprite(item);
        }

        protected abstract void PrepareProtoWeaponBow(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription);

        protected sealed override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            this.CachedWeaponTextureResourceReady = this.WeaponReadyTextureResource;

            this.PrepareProtoWeaponBow(out compatibleAmmoProtos, ref overrideDamageDescription);
        }

        private static ICharacter ClientGetItemOwnerCharacter(IItem item)
        {
            var container = item.Container;
            if (container is not null)
            {
                return container.OwnerAsCharacter;
            }

            using var tempPlayerCharacters = Api.Shared.GetTempList<ICharacter>();
            Client.Characters.GetKnownPlayerCharacters(tempPlayerCharacters);
            foreach (var character in tempPlayerCharacters.AsList())
            {
                if (ReferenceEquals(item, character.GetPublicState<ICharacterPublicState>().SelectedItem))
                {
                    // found a player character equipped with this weapon
                    return character;
                }
            }

            return null;
        }

        private static IReadOnlyDropItemsList ServerGetDroplistFor(IProtoItemAmmo protoAmmo)
        {
            if (ServerCachedDroplists.TryGetValue(protoAmmo, out var droplist))
            {
                return droplist;
            }

            droplist = new DropItemsList().Add(protoAmmo);
            ServerCachedDroplists[protoAmmo] = droplist;
            return droplist;
        }

        private void ClientRefreshWeaponInHandSprite(IItem item)
        {
            var character = ClientGetItemOwnerCharacter(item);
            if (character is null
                || !character.IsInitialized)
            {
                return;
            }

            var characterPublicState = character.GetPublicState<ICharacterPublicState>();
            if (characterPublicState is null)
            {
                return;
            }

            if (item != characterPublicState.SelectedItem)
            {
                return;
            }

            var characterClientState = character.GetClientState<BaseCharacterClientState>();
            if (characterClientState.SkeletonRenderer is null)
            {
                return;
            }

            var protoCharacterSkeleton = characterClientState.CurrentProtoSkeleton;
            protoCharacterSkeleton.ClientSetupItemInHand(
                characterClientState.SkeletonRenderer,
                this.WeaponAttachmentName,
                GetPublicState(item).IsReady
                    ? this.CachedWeaponTextureResourceReady
                    : this.CachedWeaponTextureResource);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable)]
        private void ClientRemote_OnArrowHitGround(Vector2Ushort endTilePosition)
        {
            var hitPosition = new Vector2D(endTilePosition.X + 0.5,
                                           endTilePosition.Y + 0.5);
            WeaponSystemClientDisplay.ClientAddHitSparks(
                WeaponHitSparksPresets.Firearm,
                new WeaponHitData(hitPosition: hitPosition),
                hitWorldObject: null,
                protoWorldObject: null,
                worldObjectPosition: endTilePosition.ToVector2D(),
                projectilesCount: 1,
                objectMaterial: ObjectMaterial.Wood,
                randomizeHitPointOffset: false,
                rotationAngleRad: null,
                randomRotation: false,
                drawOrder: DrawOrder.Light);
        }

        private void ServerCreateDroppedArrow(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            var shotSourcePosition = WeaponSystemClientDisplay.SharedCalculateWeaponShotWorldPositon(
                weaponCache.Character,
                weaponCache.ProtoWeapon,
                weaponCache.Character.ProtoCharacter,
                weaponCache.Character.Position,
                hasTrace: true);

            var timeToHit = WeaponSystemClientDisplay.SharedCalculateTimeToHit(
                weaponCache.ProtoWeapon.FireTracePreset
                ?? weaponCache.ProtoAmmo.FireTracePreset,
                shotSourcePosition,
                endPosition);

            ServerTimersSystem.AddAction(
                timeToHit,
                () =>
                {
                    var droplist = ServerGetDroplistFor(weaponCache.ProtoAmmo);
                    var endTilePosition = endPosition.ToVector2Ushort();

                    var result = ServerDroplistHelper.TryDropToGround(
                        droplist,
                        endTilePosition,
                        probabilityMultiplier: 1.0,
                        context: new DropItemContext(weaponCache.Character),
                        out _);

                    if (!result.IsEverythingCreated)
                    {
                        return;
                    }

                    using var charactersObserving = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetCharactersInRadius(endTilePosition,
                                                       charactersObserving,
                                                       radius: 15,
                                                       onlyPlayers: true);

                    this.CallClient(charactersObserving.AsList(),
                                    _ => _.ClientRemote_OnArrowHitGround(endTilePosition));
                });
        }
    }
}