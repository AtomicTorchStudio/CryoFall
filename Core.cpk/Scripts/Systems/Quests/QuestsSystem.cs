namespace AtomicTorch.CBND.CoreMod.Systems.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(IProtoQuest))]
    public class QuestsSystem : ProtoSystem<QuestsSystem>
    {
        public static IReadOnlyList<IProtoQuest> AllQuests;

        public override string Name => "Quests system";

        public static void ClientClaimReward(IProtoQuest protoQuest)
        {
            Instance.CallServer(_ => _.ServerRemote_ClaimReward(protoQuest));
        }

        public static void ClientMarkAsNotNew(IProtoQuest protoQuest)
        {
            var questEntry = ClientCurrentCharacterHelper.PrivateState.Quests.SharedFindQuestEntry(protoQuest, out _);
            if (!questEntry.IsNew)
            {
                return;
            }

            questEntry.IsNew = false;
            Instance.CallServer(_ => _.ServerRemote_MarkAsNotNew(protoQuest));
        }

        /// <summary>
        /// Try to set quest as completed (if player has this quest and all the requirements are satisfied).
        /// </summary>
        public static void ServerCompleteQuest(
            PlayerCharacterQuests characterQuests,
            IProtoQuest questToComplete,
            bool ignoreRequirements)
        {
            characterQuests.ServerTryClaimReward(questToComplete, ignoreRequirements);

            // success! (no exceptions thrown)
            // adding dependant quests
            foreach (var otherQuest in AllQuests)
            {
                if (!otherQuest.Prerequisites.Contains(questToComplete)
                    || !ServerArePrequisitesSatisfied(otherQuest, characterQuests))
                {
                    continue;
                }

                characterQuests.ServerTryAddQuest(otherQuest, isUnlocked: true);
            }
        }

        public static void ServerInitCharacterQuests(PlayerCharacterQuests characterQuests)
        {
            // add all quests which don't have any prerequisites or which are already satisfied
            foreach (var protoQuest in AllQuests)
            {
                var isUnlocked = ServerArePrequisitesSatisfied(protoQuest, characterQuests);
                characterQuests.ServerTryAddQuest(protoQuest, isUnlocked);
            }
        }

        protected override void PrepareSystem()
        {
            var list = Api.FindProtoEntities<IProtoQuest>();
            // topological sort will ensure there are no circular references
            list = list.TopologicalSort(funcGetDependencies: q => q.Prerequisites);
            AllQuests = list.ToArray();
        }

        private static bool ServerArePrequisitesSatisfied(IProtoQuest quest, PlayerCharacterQuests characterQuests)
        {
            if (quest.Prerequisites.Count == 0)
            {
                return true;
            }

            foreach (var requiredQuest in quest.Prerequisites)
            {
                if (!characterQuests.SharedHasCompletedQuest(requiredQuest))
                {
                    return false;
                }
            }

            return true;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, keyArgIndex: 0)]
        private void ServerRemote_ClaimReward(IProtoQuest quest)
        {
            var character = ServerRemoteContext.Character;
            var characterQuests = character.SharedGetQuests();
            ServerCompleteQuest(characterQuests, quest, ignoreRequirements: false);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, keyArgIndex: 0)]
        private void ServerRemote_MarkAsNotNew(IProtoQuest quest)
        {
            var character = ServerRemoteContext.Character;
            var characterQuests = character.SharedGetQuests();
            characterQuests.ServerRemoveNewFlag(quest);
        }
    }
}