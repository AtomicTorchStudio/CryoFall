namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemWeaponGrenadeLauncher
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeaponRanged
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public override DamageStatsComparisonPreset DamageStatsComparisonPreset
            => DamageStatsComparisonPresets.PresetRangedGrenades;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsHeavy>();

        public override void ClientOnFireModChanged(bool isFiring, uint shotsDone)
        {
            var character = ClientCurrentCharacterHelper.Character;
            var weaponState = ClientCurrentCharacterHelper.PrivateState.WeaponState;

            if (isFiring
                && !weaponState.ProtoWeapon.SharedCanFire(character, weaponState))
            {
                // cannot fire now
                weaponState.SharedSetInputIsFiring(false, shotsDone: 0);
                return;
            }

            if (!isFiring)
            {
                weaponState.CustomTargetPosition = null;
                return;
            }

            var targetPosition = Client.Input.MouseWorldPosition;

            if (Client.Input.IsKeyHeld(InputKey.Alt,        evenIfHandled: true)
                || Client.Input.IsKeyHeld(InputKey.Control, evenIfHandled: true))
            {
                // shoot in the direction as far as the range allows
                if (weaponState.WeaponCache is null)
                {
                    WeaponSystem.SharedRebuildWeaponCache(character, weaponState);
                }

                // ReSharper disable once PossibleNullReferenceException
                var rangeMax = weaponState.WeaponCache.RangeMax;
                var fromPosition = character.Position;
                var direction = targetPosition - fromPosition;
                targetPosition = fromPosition + direction.Normalized * rangeMax;
            }

            weaponState.CustomTargetPosition = targetPosition;

            this.CallServer(
                _ => _.ServerRemote_ShootOnce(targetPosition));
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            // stop firing immediately (to shoot only once per click)
            weaponState.SharedSetInputIsFiring(inputIsFiring: false,
                                               shotsDone: null);

            return base.SharedOnFire(character, weaponState);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // preload all the explosion spritesheets
            foreach (var ammoProto in this.CompatibleAmmoProtos)
            {
                if (!(ammoProto is IAmmoGrenade protoGrenade))
                {
                    continue;
                }

                foreach (var textureAtlasResource in protoGrenade.ExplosionPreset.SpriteAtlasResources)
                {
                    Client.Rendering.PreloadTextureAsync(textureAtlasResource);
                }
            }
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);

            hints.Add(ItemHints.HintShootGrenadeLauncher);
        }

        protected abstract void PrepareProtoGrenadeLauncher(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos);

        protected sealed override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            this.PrepareProtoGrenadeLauncher(out compatibleAmmoProtos);
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Ranged;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, maxCallsPerSecond: 120)]
        private void ServerRemote_ShootOnce(Vector2D targetPosition)
        {
            var character = ServerRemoteContext.Character;
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            weaponState.CustomTargetPosition = targetPosition;
            // perform one more shot
            weaponState.SharedSetInputIsFiring(inputIsFiring: false,
                                               weaponState.ShotsDone + 1);
        }
    }

    public abstract class ProtoItemWeaponGrenadeLauncher
        : ProtoItemWeaponGrenadeLauncher
            <WeaponPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}