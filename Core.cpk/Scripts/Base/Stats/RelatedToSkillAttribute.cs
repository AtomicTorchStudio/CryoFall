namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class RelatedToSkillAttribute : Attribute
    {
        private readonly Type skillType;

        private IProtoSkill cachedProtoSkill;

        public RelatedToSkillAttribute(Type skillType)
        {
            this.skillType = skillType;
        }

        [CanBeNull]
        public IProtoSkill ProtoSkill
            => this.cachedProtoSkill ??= this.GetProtoSkill();

        private IProtoSkill GetProtoSkill()
        {
            var protoEntity = Api.Shared.GetProtoEntityAbstract(this.skillType);
            if (protoEntity is IProtoSkill protoSkill)
            {
                return protoSkill;
            }

            Api.Logger.Error("Type is not a skill prototype: " + this.skillType);
            return null;
        }
    }
}