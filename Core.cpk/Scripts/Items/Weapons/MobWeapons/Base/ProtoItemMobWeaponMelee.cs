namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoItemMobWeaponMelee : ProtoItemWeaponMelee
    {
        public override IProtoItem BaseProtoItem => null;

        public override bool CanDamageStructures => false;

        public override string Description => null;

        public override uint DurabilityMax => 0;

        public override bool IsLoopedAttackAnimation => false;

        public sealed override bool IsSkinnable => false;

        public override string Name => this.ShortId;

        public override double ReadyDelayDuration => 0;

        public override double SpecialEffectProbability =>
            1; // Must always be 1 for all animal weapons. Individual effects will be rolled in the effect function.

        protected override ProtoSkillWeapons WeaponSkill => null;

        protected override TextureResource WeaponTextureResource => null;

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview = false)
        {
            // do nothing
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            return character.ProtoCharacter is IProtoCharacterMob;
        }

        protected override ITextureResource PrepareIcon()
        {
            return null;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.SpecialUseSkeletonSound;
        }
    }
}