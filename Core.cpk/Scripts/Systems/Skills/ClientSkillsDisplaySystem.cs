namespace AtomicTorch.CBND.CoreMod.Systems.Skills
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ClientSkillsDisplaySystem : ProtoSystem<ClientSkillsDisplaySystem>
    {
        public static Task<double> ClientGetSkillExperience(IProtoSkill skill)
        {
            return Instance.CallServer(
                _ => _.ServerRemote_GetSkillExperience(skill));
        }

        // TODO: clientMaxSendQueueSize must match the number of skills in the game plus few extra
        [RemoteCallSettings(timeInterval: 0.05, clientMaxSendQueueSize: 50)]
        private double ServerRemote_GetSkillExperience(IProtoSkill skill)
        {
            return ServerRemoteContext.Character.SharedGetSkill(skill).Experience;
        }
    }
}