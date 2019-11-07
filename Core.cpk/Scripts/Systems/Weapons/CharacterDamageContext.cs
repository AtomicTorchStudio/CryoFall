namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public readonly struct CharacterDamageContext : IDisposable
    {
        public readonly ICharacter AttackerCharacter;

        public readonly ICharacter DamagedCharacter;

        public readonly ProtoSkillWeapons ProtoWeaponSkill;

        private CharacterDamageContext(
            ICharacter attackerCharacter,
            ICharacter damagedCharacter,
            ProtoSkillWeapons protoWeaponSkill)
        {
            this.AttackerCharacter = attackerCharacter;
            this.DamagedCharacter = damagedCharacter;
            this.ProtoWeaponSkill = protoWeaponSkill;
        }

        public static CharacterDamageContext Current { get; private set; }

        public static CharacterDamageContext Create(
            ICharacter attackerCharacter,
            ICharacter damagedCharacter,
            ProtoSkillWeapons protoWeaponSkill)
        {
            return Current = new CharacterDamageContext(attackerCharacter, damagedCharacter, protoWeaponSkill);
        }

        public void Dispose()
        {
            Current = default;
        }
    }
}