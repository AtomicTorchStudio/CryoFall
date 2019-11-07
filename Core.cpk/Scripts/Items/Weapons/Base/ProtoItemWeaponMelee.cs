namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemWeaponMelee
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeapon
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWeaponMelee
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public override ushort AmmoCapacity => 0;

        public override double AmmoReloadDuration => 0;

        public override string CharacterAnimationAimingName => null;

        public override double DamageApplyDelay => 0.075;

        public override double FireAnimationDuration => 0.6;

        public override double FireInterval => this.FireAnimationDuration;

        public override ITextureResource Icon => new TextureResource("Items/Weapons/Melee/" + this.GetType().Name);

        public override double ReadyDelayDuration => 0.5;

        public override string WeaponAttachmentName => "WeaponMelee";

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public static string GetMeleeCharacterAnimationNameFire(ICharacter character)
        {
            var pi = MathConstants.PI;
            var piQuarter = pi / 4;

            var angle = character.ProtoCharacter.SharedGetRotationAngleRad(character);

            if (angle > piQuarter
                && angle < pi - piQuarter)
            {
                return "AttackMeleeVertical";
            }

            if (angle > pi + piQuarter
                && angle < 2 * pi - piQuarter)
            {
                return "AttackMeleeVertical";
            }

            return "AttackMeleeHorizontal";
        }

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return GetMeleeCharacterAnimationNameFire(character);
        }

        protected override ReadOnlySoundPreset<ObjectSoundMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Melee;
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemWeaponMelee;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponMelee;
        }

        protected override void ServerOnDegradeWeapon(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            List<WeaponHitData> hitObjects)
        {
            if (hitObjects.Count == 0)
            {
                // no objects were hit
                return;
            }

            var decrease = this.DurabilityDecreasePerAction;
            foreach (var hit in hitObjects)
            {
                var protoObject = hit.WorldObject.ProtoWorldObject;
                if (protoObject is IProtoObjectWall
                    || protoObject is IProtoObjectDoor
                    || protoObject is IProtoObjectTradingStation)
                {
                    // hit wall, door or station
                    decrease *= 5;
                    break;
                }
            }

            ItemDurabilitySystem.ServerModifyDurability(
                weaponItem,
                delta: -decrease);
        }
    }

    public abstract class ProtoItemWeaponMelee
        : ProtoItemWeaponMelee
            <WeaponPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}