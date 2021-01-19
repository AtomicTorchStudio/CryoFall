namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterIdleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoSkill<TFlagEffectsEnum> : ProtoEntity, IProtoSkill
        where TFlagEffectsEnum : struct, Enum
    {
        private double[] experienceRequirementForLevels;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoSkill()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("Skill", StringComparison.Ordinal))
            {
                throw new Exception("Skill class name must start with \"Skill\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("Skill".Length);
            this.Icon = new TextureResource("Skills/Icons/" + this.ShortId);
            this.Picture = new TextureResource("Skills/Pictures/" + this.ShortId);
        }

        /// <summary>
        /// Skill category.
        /// </summary>
        public ProtoSkillCategory Category { get; private set; }

        /// <summary>
        /// Text description.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// By default skills do not modify the conversion rate (it is set in the tech constants),
        /// but if need be it can be modified individually, such as reduced for athletics skill
        /// </summary>
        public abstract double ExperienceToLearningPointsConversionMultiplier { get; }

        public IReadOnlyList<FlagEffect> FlagEffects { get; private set; }

        public bool HasStatEffects { get; private set; }

        public ITextureResource Icon { get; }

        /// <summary>
        /// If true, this skill will share the gained learning with the party members (if there are other party members).
        /// </summary>
        public abstract bool IsSharingLearningPointsWithPartyMembers { get; }

        public byte MaxLevel { get; private set; }

        public ITextureResource Picture { get; }

        public sealed override string ShortId { get; }

        public IReadOnlyList<StatEffect> StatEffects { get; private set; }

        /// <summary>
        /// Default formula to calculate experience per level.
        /// First level requires 5000 exp.
        /// Second and next levels require accordingly 2160 exp, 3360, 4640, 6000, 7440...
        /// </summary>
        protected static FuncExperiencePerLevel DefaultFormulaExperiencePerLevel { get; }
            = level =>
                  level == 1
                      ? 5000
                      : MathHelper.TruncateLastDigits(
                          level * (level + 25) * 40,
                          digits: 2);

        public byte CalculateLevelForExperience(double experience)
        {
            for (byte level = 0; level < this.MaxLevel; level++)
            {
                var minExp = this.experienceRequirementForLevels[level];
                if (experience < minExp)
                {
                    return level;
                }
            }

            return this.MaxLevel;
        }

        public byte ClampLevel(byte level)
        {
            return level >= this.MaxLevel
                       ? this.MaxLevel
                       : level;
        }

        public void FillEffectsCache(BaseStatsDictionary statsCache, byte level)
        {
            if (!this.HasStatEffects)
            {
                return;
            }

            // this temp cache will be taken only when necessarily
            TempStatsCache tempStatsCache = null;

            foreach (var statEffect in this.StatEffects)
            {
                if (level < statEffect.Level)
                {
                    // level is lower than required
                    continue;
                }

                tempStatsCache ??= TempStatsCache.GetFromPool();

                var statName = statEffect.StatName;
                tempStatsCache.AddValue(this, statName, statEffect.CalcTotalValueBonus(level));
                tempStatsCache.AddPercent(this, statName, statEffect.CalcTotalPercentBonus(level));
            }

            if (tempStatsCache is not null)
            {
                statsCache.Merge(tempStatsCache);
                tempStatsCache.Dispose();
            }
        }

        public IEnumerable<ISkillEffect> GetEffects()
        {
            foreach (var effect in this.StatEffects)
            {
                yield return effect;
            }

            foreach (var effect in this.FlagEffects)
            {
                yield return effect;
            }
        }

        public double GetExperienceForLevel(byte level)
        {
            if (level < 1)
            {
                return 0;
            }

            level = this.ClampLevel(level);
            return this.experienceRequirementForLevels[level - 1];
        }

        public void ServerOnSkillExperienceAdded(
            ICharacter character,
            double experienceAdded,
            byte currentLevel)
        {
            var multiplier = this.ExperienceToLearningPointsConversionMultiplier
                             * TechConstants.ServerSkillExperienceToLearningPointsConversionMultiplier;

            // apply reversed experience gain multiplier so faster/slower skill exp gain speed will not affect LP gain speed
            multiplier /= TechConstants.ServerSkillExperienceGainMultiplier;

            if (multiplier <= 0
                || double.IsNaN(multiplier))
            {
                return;
            }

            // reduce LP gain proportionally to the skill level
            var lpRateMultiplier = MathHelper.Lerp(1,
                                                   TechConstants.SkillLearningPointMultiplierAtMaximumLevel,
                                                   currentLevel / (double)this.MaxLevel);

            multiplier *= lpRateMultiplier;

            if (multiplier <= 0)
            {
                return;
            }

            var learningPointsToAdd = experienceAdded * multiplier;

            var partyMembersNames = this.IsSharingLearningPointsWithPartyMembers
                                        ? PartySystem.ServerGetPartyMembersReadOnly(character)
                                        : Array.Empty<string>();

            if (partyMembersNames.Count <= 1)
            {
                // no experience share, no party or a single member party - add all LP to the current character
                character.SharedGetTechnologies()
                         .ServerAddLearningPoints(learningPointsToAdd);
                return;
            }

            using var onlinePartyMembers = Api.Shared.GetTempList<ICharacter>();
            foreach (var partyMemberName in partyMembersNames)
            {
                var partyMember = Server.Characters.GetPlayerCharacter(partyMemberName);
                if (partyMember is null)
                {
                    continue;
                }

                if (ReferenceEquals(partyMember, character)
                    || (partyMember.ServerIsOnline
                        && !CharacterIdleSystem.ServerIsIdlePlayer(partyMember)))
                {
                    onlinePartyMembers.Add(partyMember);
                }
            }

            if (onlinePartyMembers.Count <= 1
                || PartyConstants.PartyLearningPointsSharePercent == 0)
            {
                // no party, or a single member party, or no party share %
                // - add all LP to the current character
                character.SharedGetTechnologies()
                         .ServerAddLearningPoints(learningPointsToAdd);
                return;
            }

            // player has a party
            // add only a share of LP to current character
            var currentCharacterLearningPointsShare = learningPointsToAdd * (1 - PartyConstants.PartyLearningPointsSharePercent);
            character.SharedGetTechnologies()
                     .ServerAddLearningPoints(currentCharacterLearningPointsShare);

            // distribute the rest equally to the other party members
            var learningPointsShare = learningPointsToAdd
                                      * PartyConstants.PartyLearningPointsSharePercent
                                      / (onlinePartyMembers.Count - 1);

            foreach (var partyMember in onlinePartyMembers.AsList())
            {
                if (!ReferenceEquals(partyMember, character))
                {
                    partyMember.SharedGetTechnologies()
                               .ServerAddLearningPoints(learningPointsShare);
                }
            }
        }

        public void ServerUpdateSkillData(SkillLevelData data)
        {
            var level = this.ClampLevel(data.Level);
            var expForCurrentLevel = this.GetExperienceForLevel(level);
            var exp = data.Experience;

            if (exp >= expForCurrentLevel
                && exp < data.ExperienceForNextLevel)
            {
                // current level matches its range
                return;
            }

            // recalculate level and experience required for next level
            data.Level = this.CalculateLevelForExperience(exp);
            var nextLevel = data.Level + 1;
            data.ExperienceForNextLevel = nextLevel <= this.MaxLevel
                                              ? this.GetExperienceForLevel((byte)nextLevel)
                                              : double.MaxValue;
        }

        public bool SharedCheckFlagForLevel(TFlagEffectsEnum flag, byte level)
        {
            foreach (var entry in this.FlagEffects)
            {
                if (flag.Equals(entry.Flag))
                {
                    return level >= entry.Level;
                }
            }

            // should be impossible because we validate FlagEffects list during PrepareProtoSkill()
            return false;
        }

        protected static TProtoSkillCategory GetCategory<TProtoSkillCategory>()
            where TProtoSkillCategory : ProtoSkillCategory, new()
        {
            return Api.GetProtoEntity<TProtoSkillCategory>();
        }

        protected sealed override void PrepareProto()
        {
            base.PrepareProto();

            var config = new SkillConfig();
            config.MaxLevel = 20;

            config.FormulaExperiencePerLevel = DefaultFormulaExperiencePerLevel;

            this.PrepareProtoSkill(config);
            config.Validate(this);

            this.Category = config.Category;
            this.MaxLevel = config.MaxLevel.Value;
            this.StatEffects = config.GetStatEffects();
            this.FlagEffects = config.GetFlagEffects();
            this.HasStatEffects = this.StatEffects.Count > 0;

            this.experienceRequirementForLevels = CalculateExperienceTable(
                this.MaxLevel,
                config.FormulaExperiencePerLevel);
        }

        protected abstract void PrepareProtoSkill(SkillConfig config);

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static double[] CalculateExperienceTable(byte maxLevel, FuncExperiencePerLevel formula)
        {
            var result = new double[maxLevel];
            var prevExp = 0d;
            for (byte level = 0; level < maxLevel; level++)
            {
                var expDelta = Math.Round(formula((byte)(level + 1)));
                var exp = prevExp + expDelta;
                result[level] = exp;

                if (expDelta <= 0)
                {
                    var expTable = result
                                   .Take(level)
                                   .Select((e, index) => "   level=" + (index + 1) + " exp=" + e.ToString("F2"));

                    throw new Exception(
                        "There is a bug in the experience for level calculation formula:"
                        + Environment.NewLine
                        + "The experience required for upgrading from level "
                        + (level - 1)
                        + " to level "
                        + level
                        + " is lower or equal to zero."
                        + Environment.NewLine
                        + "The experience table (until the bugged level):"
                        + Environment.NewLine
                        + expTable.GetJoinedString(Environment.NewLine));
                }

                prevExp = exp;
            }

            return result;
        }

        public struct FlagEffect : IFlagEffect
        {
            public FlagEffect(TFlagEffectsEnum flag, byte level, string description)
            {
                this.Flag = flag;
                this.Level = level;
                this.Description = description
                                   ?? (flag as Enum).GetAttribute<DescriptionAttribute>()?.Description
                                   ?? throw new Exception("There is no [Description] attribute for " + flag);
            }

            public string Description { get; }

            public TFlagEffectsEnum Flag { get; }

            public byte Level { get; }
        }

        protected class SkillConfig
        {
            public ProtoSkillCategory Category;

            /// <summary>
            /// Formula to calculate experience required per level.
            /// </summary>
            public FuncExperiencePerLevel FormulaExperiencePerLevel;

            public byte? MaxLevel;

            private readonly List<FlagEffect> flagEffects = new();

            private readonly List<StatEffect> statEffects = new();

            public void AddFlagEffect(TFlagEffectsEnum flag, /*string description, */ byte level)
            {
                foreach (var flagEffect in this.flagEffects)
                {
                    if (flag.Equals(flagEffect.Flag))
                    {
                        throw new Exception("Flag " + flag + "is already registered");
                    }
                }

                this.flagEffects.Add(new FlagEffect(flag, level, description: null));
            }

            public void AddStatEffect(
                StatName statName,
                byte level = 1,
                FuncBonusAtLevel formulaValueBonus = null,
                FuncBonusAtLevel formulaPercentBonus = null,
                double valueBonus = 0,
                double percentBonus = 0,
                bool displayTotalValue = false)
            {
                this.statEffects.Add(
                    new StatEffect(
                        statName,
                        level,
                        formulaValueBonus,
                        formulaPercentBonus,
                        valueBonus,
                        percentBonus,
                        displayTotalValue));
            }

            public IReadOnlyList<FlagEffect> GetFlagEffects()
            {
                return this.flagEffects;
            }

            public IReadOnlyList<StatEffect> GetStatEffects()
            {
                return this.statEffects;
            }

            public void Validate(ProtoSkill<TFlagEffectsEnum> protoSkill)
            {
                try
                {
                    if (this.Category is null)
                    {
                        throw new Exception("Skill category is not set");
                    }

                    if (this.FormulaExperiencePerLevel is null)
                    {
                        throw new Exception(nameof(this.FormulaExperiencePerLevel) + " is not set");
                    }

                    if (this.MaxLevel is null)
                    {
                        throw new Exception("MaxLevel is not set");
                    }

                    if (this.MaxLevel < 1)
                    {
                        throw new Exception("MaxLevel should be >= 1");
                    }

                    if (this.MaxLevel > 100)
                    {
                        throw new Exception("MaxLevel should be <= 100");
                    }

                    var enumType = typeof(TFlagEffectsEnum);
                    var hasFlagEnum = enumType != typeof(ProtoSkill.NoFlags);
                    if (!hasFlagEnum)
                    {
                        return;
                    }

                    // check flags enum
                    if (enumType.DeclaringType != protoSkill.GetType())
                    {
                        throw new Exception(
                            $"The flags enum type \"{enumType.FullName}\" is defined in different Skill class.It MUST be defined inside this skill class. See how other skills are implemented.");
                    }

                    if (!enumType.IsEnum)
                    {
                        throw new Exception($"Flags type \"{enumType.FullName}\" should be an enum.");
                    }

                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    if (enumType.Name != "Flags")
                    {
                        throw new Exception(
                            $"Flags type \"{enumType.FullName}\" should have name \"Flags\", not \"{enumType.Name}\".");
                    }

                    if (!enumType.IsNestedPublic)
                    {
                        throw new Exception($"Flags type \"{enumType.FullName}\" should be public.");
                    }

                    // all the flags should be used during setup of the proto skill config
                    var enumValues = EnumExtensions.GetValues<TFlagEffectsEnum>();
                    if (this.flagEffects.Count == enumValues.Length)
                    {
                        return;
                    }

                    var notUsedFlags = enumValues.Where(
                        flag => !this.flagEffects.Any(entry => flag.Equals(entry.Flag)));

                    throw new Exception(
                        $"The skill flag(s) \"{notUsedFlags.GetJoinedString()}\" is/are not used during PrepareProtoSkill() call. It might be an error - if the flag is not used by skill, remove it from the flags enum.");
                }
                catch (Exception ex)
                {
                    Logger.Error($"{protoSkill} - found an error.{Environment.NewLine}{ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Skill without flag effects.
    /// </summary>
    public abstract class ProtoSkill : ProtoSkill<ProtoSkill.NoFlags>
    {
        public enum NoFlags
        {
        }
    }
}