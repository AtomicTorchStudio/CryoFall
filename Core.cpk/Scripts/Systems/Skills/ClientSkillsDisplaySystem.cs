namespace AtomicTorch.CBND.CoreMod.Systems.Skills
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ClientSkillsDisplaySystem : ProtoSystem<ClientSkillsDisplaySystem>
    {
        public override string Name => "Skill display system";

        public static Task<double> ClientGetSkillExperience(IProtoSkill skill)
        {
            return Instance.CallServer(
                _ => _.ServerRemote_GetSkillExperience(skill));
        }

        private double ServerRemote_GetSkillExperience(IProtoSkill skill)
        {
            return ServerRemoteContext.Character.SharedGetSkill(skill).Experience;
        }
    }
}