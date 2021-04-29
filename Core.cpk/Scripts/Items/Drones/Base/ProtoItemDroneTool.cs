namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ProtoItemDroneTool
        : ProtoItemWeaponRanged,
          IProtoItemToolMining,
          IProtoItemToolWoodcutting
    {
        protected ProtoItemDroneTool()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            this.Name = this.ShortId;
        }

        public sealed override ushort AmmoCapacity => 0;

        public sealed override double AmmoReloadDuration => 0;

        public abstract double DamageToMinerals { get; }

        public abstract double DamageToTree { get; }

        public sealed override string Description => string.Empty;

        public sealed override uint DurabilityMax => 0;

        public sealed override string Name { get; }

        public override (float min, float max) SoundPresetWeaponDistance
            => (SoundConstants.AudioListenerMinDistance, SoundConstants.AudioListenerMaxDistance);

        protected override ProtoSkillWeapons WeaponSkill => null;

        public double ServerGetDamageToMineral(IStaticWorldObject targetObject)
        {
            return this.DamageToMinerals;
        }

        public double ServerGetDamageToTree(IStaticWorldObject targetObject)
        {
            return this.DamageToTree;
        }

        protected override ITextureResource PrepareIcon()
        {
            return null;
        }

        protected sealed override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
        }

        /// <summary>
        /// The damage description is never actually used.
        /// </summary>
        protected sealed override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;
            overrideDamageDescription = new DamageDescription(damageValue: 0,
                                                              armorPiercingCoef: 0,
                                                              finalDamageMultiplier: 1,
                                                              rangeMax: 1,
                                                              new DamageDistribution(DamageType.Heat, 1));
        }
    }
}