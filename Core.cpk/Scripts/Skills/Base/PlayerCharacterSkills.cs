namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterSkills : BaseNetObject
    {
        public delegate void CharacterSkillLevelChangedDelegate(
            ICharacter character,
            IProtoSkill skill,
            SkillLevelData skillLevelData);

        public static event CharacterSkillLevelChangedDelegate CharacterSkillLevelChanged;

        public ICharacter Character => (ICharacter)this.GameObject;

        [SyncToClient]
        public NetworkSyncDictionary<IProtoSkill, SkillLevelData> Skills { get; }
            = new NetworkSyncDictionary<IProtoSkill, SkillLevelData>();

        public SkillLevelData ServerAddSkillExperience<TProtoSkill>(double experience)
            where TProtoSkill : IProtoSkill, new()
        {
            var skill = Api.GetProtoEntity<TProtoSkill>();
            return this.ServerAddSkillExperience(skill, experience);
        }

        public SkillLevelData ServerAddSkillExperience(IProtoSkill skill, double experience)
        {
            Api.ValidateIsServer();
            if (experience <= 0)
            {
                throw new ArgumentException("Experience to add should be larger than zero.", nameof(experience));
            }

            experience *= TechConstants.SkillExperienceGainMultiplier;

            if (!this.Skills.TryGetValue(skill, out var skillLevelData))
            {
                skillLevelData = this.ServerSetSkillExperience(skill, experience);
                return skillLevelData;
            }

            var oldLevel = skillLevelData.Level;

            var newExp = experience + skillLevelData.Experience;
            skillLevelData.Experience = newExp;

            if (newExp >= skillLevelData.ExperienceForNextLevel)
            {
                var previousLevel = skillLevelData.Level;

                skill.ServerUpdateSkillData(skillLevelData);

                if (previousLevel != skillLevelData.Level)
                {
                    Api.SafeInvoke(
                        () => CharacterSkillLevelChanged?.Invoke(this.Character, skill, skillLevelData));
                }
            }

            if (skill.HasStatEffects
                && oldLevel != skillLevelData.Level)
            {
                this.ServerSetCharacterFullStatsCacheDirty();
            }

            skill.ServerOnSkillExperienceAdded(this.Character, experience, skillLevelData.Level);
            return skillLevelData;
        }

        public SkillLevelData ServerDebugSetSkill<TProtoSkill>(byte level)
            where TProtoSkill : IProtoSkill, new()
        {
            var skill = Api.GetProtoEntity<TProtoSkill>();
            return this.ServerDebugSetSkill(skill, level);
        }

        public SkillLevelData ServerDebugSetSkill(IProtoSkill skill, byte level)
        {
            Api.ValidateIsServer();

            level = skill.ClampLevel(level);
            var exp = skill.GetExperienceForLevel(level);
            return this.ServerSetSkillExperience(skill, exp);
        }

        public SkillLevelData ServerDebugSetSkillExperience(IProtoSkill skill, double experience)
        {
            Api.ValidateIsServer();
            return this.ServerSetSkillExperience(skill, experience);
        }

        public void ServerReset()
        {
            Api.ValidateIsServer();

            if (this.Skills.Count > 0)
            {
                foreach (var pair in this.Skills)
                {
                    Api.SafeInvoke(
                        () => CharacterSkillLevelChanged?.Invoke(this.Character, pair.Key, pair.Value));
                }

                this.Skills.Clear();
            }

            this.ServerSetCharacterFullStatsCacheDirty();
        }

        public void SharedFillEffectsCache(BaseStatsDictionary statsCache)
        {
            foreach (var pair in this.Skills)
            {
                var skill = pair.Key;
                var skillLevel = pair.Value.Level;
                if (skillLevel > 0)
                {
                    skill.FillEffectsCache(statsCache, skillLevel);
                }
            }
        }

        public SkillLevelData SharedGetSkill<TProtoSkill>()
            where TProtoSkill : IProtoSkill, new()
        {
            var skill = Api.GetProtoEntity<TProtoSkill>();
            return this.SharedGetSkill(skill);
        }

        public SkillLevelData SharedGetSkill(IProtoSkill skill)
        {
            return this.Skills.TryGetValue(skill, out var skillLevelData)
                       ? skillLevelData
                       : new SkillLevelData();
        }

        public bool SharedHasSkill<TProtoSkill>(byte level)
            where TProtoSkill : IProtoSkill, new()
        {
            var skill = Api.GetProtoEntity<TProtoSkill>();
            return this.SharedHasSkill(skill, level);
        }

        public bool SharedHasSkill(IProtoSkill skill, byte level)
        {
            var data = this.SharedGetSkill(skill);
            return data.Level >= level;
        }

        private void ServerSetCharacterFullStatsCacheDirty()
        {
            this.Character.SharedSetFinalStatsCacheDirty();
        }

        private SkillLevelData ServerSetSkillExperience(IProtoSkill skill, double newExperience)
        {
            var needToAdd = false;
            if (!this.Skills.TryGetValue(skill, out var skillLevelData))
            {
                needToAdd = true;
                skillLevelData = new SkillLevelData();
            }

            skillLevelData.Experience = newExperience;
            skill.ServerUpdateSkillData(skillLevelData);

            if (needToAdd)
            {
                this.Skills[skill] = skillLevelData;
            }

            if (skill.HasStatEffects)
            {
                this.ServerSetCharacterFullStatsCacheDirty();
            }

            Api.SafeInvoke(
                () => CharacterSkillLevelChanged?.Invoke(this.Character, skill, skillLevelData));

            return skillLevelData;
        }
    }
}