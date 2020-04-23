namespace AtomicTorch.CBND.CoreMod.Skills
{
    public abstract class ProtoSkillWeaponsRanged : ProtoSkillWeapons
    {
        protected sealed override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryCombat>();
            this.PrepareProtoWeaponsSkillRanged(config);
        }

        protected abstract void PrepareProtoWeaponsSkillRanged(SkillConfig config);
    }
}