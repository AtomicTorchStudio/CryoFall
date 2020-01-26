namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoSkillWeapons : ProtoSkill
    {
        /// <summary>
        /// This functions like a small bonus to the person who dealt the killing blow/shot.
        /// </summary>
        public abstract double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier { get; }

        /// <summary>
        /// This XP is directly proportional to dealt damage.
        /// </summary>
        public abstract double ExperienceAddedPerDamageDoneMultiplier { get; }

        /// <summary>
        /// This is intended to reward experience per ammo expended. Basically resource->exp conversion.
        /// </summary>
        public abstract double ExperienceAddedPerShot { get; }

        public sealed override bool IsSharingLearningPointsWithPartyMembers => true;

        public abstract StatName StatNameDamageBonusMultiplier { get; }

        public abstract StatName StatNameDegrationRateMultiplier { get; }

        public abstract StatName? StatNameReloadingSpeedMultiplier { get; }

        public abstract StatName StatNameSpecialEffectChanceMultiplier { get; }

        public void ServerOnDamageApplied(
            PlayerCharacterSkills skills,
            IWorldObject damagedObject,
            double damageApplied)
        {
            if (damagedObject is ICharacter)
            {
                // apply experience only when the damage is applied to a character
                skills.ServerAddSkillExperience(this, damageApplied * this.ExperienceAddedPerDamageDoneMultiplier);
            }
        }

        public void ServerOnKill(PlayerCharacterSkills skills, ICharacter killedCharacter)
        {
            var maxHealth = killedCharacter.SharedGetFinalStatValue(StatName.HealthMax);
            skills.ServerAddSkillExperience(
                this,
                experience: maxHealth * this.ExperienceAddedOnKillPerMaxEnemyHealthMultiplier);
        }

        public void ServerOnShot(PlayerCharacterSkills skills)
        {
            if (this.ExperienceAddedPerShot > 0)
            {
                skills.ServerAddSkillExperience(this, this.ExperienceAddedPerShot);
            }
        }
    }
}