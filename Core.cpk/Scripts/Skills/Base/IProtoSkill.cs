namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoSkill : IProtoEntity
    {
        /// <summary>
        /// Skill category.
        /// </summary>
        ProtoSkillCategory Category { get; }

        string Description { get; }

        bool HasStatEffects { get; }

        ITextureResource Icon { get; }

        byte MaxLevel { get; }

        ITextureResource Picture { get; }

        byte ClampLevel(byte level);

        void FillEffectsCache(BaseStatsDictionary statsCache, byte skillLevel);

        IEnumerable<ISkillEffect> GetEffects();

        double GetExperienceForLevel(byte level);

        void ServerOnSkillExperienceAdded(ICharacter character, double experienceAdded, byte currentLevel);

        void ServerUpdateSkillData(SkillLevelData data);
    }
}