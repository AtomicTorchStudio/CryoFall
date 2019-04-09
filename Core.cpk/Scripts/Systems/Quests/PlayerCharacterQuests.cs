namespace AtomicTorch.CBND.CoreMod.Systems.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterQuests : BaseNetObject
    {
        private readonly List<CharacterQuestEntry> serverLockedQuests
            = new List<CharacterQuestEntry>();

        static PlayerCharacterQuests()
        {
            PartySystem.ServerCharacterJoinedOrLeftParty += ServerCharacterJoinedOrLeftPartyHandler;
        }

        public ICharacter Character => (ICharacter)this.GameObject;

        [SyncToClient]
        public NetworkSyncList<CharacterQuestEntry> Quests { get; }
            = new NetworkSyncList<CharacterQuestEntry>();

        public void ServerInit()
        {
            Process(this.Quests);
            Process(this.serverLockedQuests);

            QuestsSystem.ServerInitCharacterQuests(this);

            void Process(IList<CharacterQuestEntry> list)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var entry = list[index];
                    if (entry.Quest == null)
                    {
                        // invalid quest entry
                        list.RemoveAt(index);
                        index--;
                        continue;
                    }

                    if (entry.IsCompleted)
                    {
                        continue;
                    }

                    entry.ServerEnsureRequirementsStateIsValid();
                    entry.ServerCreateActiveRequirementStates(this.Character);
                }
            }
        }

        public void ServerRemoveNewFlag(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out var isUnlocked);
            if (questEntry == null)
            {
                throw new Exception("No quest entry for: " + quest);
            }

            if (!isUnlocked)
            {
                throw new Exception("Cannot remove the new flag for the locked quest entry: " + quest);
            }

            questEntry.IsNew = false;
        }

        public void ServerReset()
        {
            foreach (var questEntry in this.Quests)
            {
                questEntry.ServerDestroy();
            }

            this.Quests.Clear();

            foreach (var questEntry in this.serverLockedQuests)
            {
                questEntry.ServerDestroy();
            }

            this.serverLockedQuests.Clear();

            Api.Logger.Important("Quests reset", this.Character);
            this.ServerInit();
        }

        public void ServerTryAddQuest(IProtoQuest quest, bool isUnlocked)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out _);
            if (questEntry != null)
            {
                // already has an entry
                if (isUnlocked
                    && this.serverLockedQuests.Remove(questEntry))
                {
                    // this is a locked entry and it was removed - it will be added to the unlocked quests list
                }
                else
                {
                    // no need to add the quest entry
                    return;
                }
            }
            else
            {
                questEntry = new CharacterQuestEntry(quest);
            }

            if (isUnlocked)
            {
                this.Quests.Add(questEntry);
            }
            else
            {
                this.serverLockedQuests.Add(questEntry);
            }

            //Api.Logger.Important($"Quest added: {quest.ShortId} as {(isUnlocked ? "unlocked" : "locked")}",
            //                     this.Character);

            questEntry.ServerCreateActiveRequirementStates(this.Character);
        }

        public void ServerTryClaimReward(IProtoQuest quest, bool ignoreRequirements)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out bool questEntryIsUnlocked);
            if (questEntry == null)
            {
                throw new Exception("No quest entry found for: " + quest);
            }

            if (!questEntryIsUnlocked)
            {
                throw new Exception("The quest is not unlocked " + quest);
            }

            if (!ignoreRequirements
                && !questEntry.AreAllRequirementsSatisfied)
            {
                throw new Exception("The quest requirements are not satisfied " + quest);
            }

            if (questEntry.IsCompleted)
            {
                throw new Exception("The quest is already completed and the reward is already claimed " + quest);
            }

            questEntry.ServerSetCompleted();

            var rewardLearningPoints = quest.RewardLearningPoints;
            this.Character.SharedGetTechnologies()
                .ServerAddLearningPoints(rewardLearningPoints);

            Api.Logger.Important(
                $"Quest completed and reward claimed: {quest.ShortId}. Learning points added: {rewardLearningPoints}",
                this.Character);
        }

        public void ServerTryRemoveQuest(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out _);
            if (questEntry == null)
            {
                // no such entry
                return;
            }

            questEntry.ServerDestroy();
            this.Quests.Remove(questEntry);
            this.serverLockedQuests.Remove(questEntry);
            Api.Logger.Important("Quest removed: " + quest.ShortId, this.Character);
        }

        public CharacterQuestEntry SharedFindQuestEntry(IProtoQuest quest, out bool isUnlocked)
        {
            foreach (var entry in this.Quests)
            {
                if (entry.Quest == quest)
                {
                    isUnlocked = true;
                    return entry;
                }
            }

            if (Api.IsServer)
            {
                foreach (var entry in this.serverLockedQuests)
                {
                    if (entry.Quest == quest)
                    {
                        isUnlocked = false;
                        return entry;
                    }
                }
            }

            isUnlocked = false;
            return null;
        }

        public bool SharedHasCompletedQuest(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out var isUnlocked);
            if (questEntry == null
                || !isUnlocked)
            {
                // no such entry
                return false;
            }

            return questEntry.IsCompleted;
        }

        public bool SharedHasCompletedRequirement(IQuestRequirement requirement)
        {
            var quest = requirement.Quest;
            Api.Assert(quest != null, "Requirement must have an associated quest");

            var questEntry = this.SharedFindQuestEntry(quest, out var isUnlocked);
            if (questEntry == null
                || !isUnlocked)
            {
                // no such entry
                return false;
            }

            if (questEntry.IsCompleted
                || questEntry.AreAllRequirementsSatisfied)
            {
                // all requirements are/were satisfied
                return true;
            }

            // find a state for this requirement and check whether it's satisfied
            var indexOfRequirement = -1;
            var requirements = quest.Requirements;
            if (requirements == null)
            {
                return false;
            }

            for (var index = 0; index < requirements.Count; index++)
            {
                var r = requirements[index];
                if (r != requirement)
                {
                    continue;
                }

                indexOfRequirement = index;
                break;
            }

            Api.Assert(indexOfRequirement >= 0, "Requirement not exist in quest");
            var requirementState = questEntry.RequirementStates[indexOfRequirement];
            return requirementState.IsSatisfied;
        }

        private static void ServerCharacterJoinedOrLeftPartyHandler(ICharacter character)
        {
            var playerCharacterQuests = character.SharedGetQuests();
            foreach (var quest in playerCharacterQuests.Quests)
            {
                quest.ServerRefreshAllRequirementsSatisfied(character);
            }
        }

        public class CharacterQuestEntry : BaseNetObject
        {
            [NonSerialized]
            private List<ServerCharacterActiveQuestRequirement> serverActiveRequirements;

            public CharacterQuestEntry(IProtoQuest quest)
            {
                this.Quest = quest;
                this.ServerSetupRequirementStates();
            }

            /// <summary>
            /// When all requirements are satisfied, player can claim the reward and then <see cref="IsCompleted" /> should be set to
            /// true.
            /// </summary>
            [SyncToClient]
            public bool AreAllRequirementsSatisfied { get; private set; }

            //public ICharacter Character => (ICharacter)this.GameObject;

            [SyncToClient]
            public bool IsCompleted { get; private set; }

            [SyncToClient(isSendChanges: false)]
            public bool IsNew { get; set; } = true;

            [SyncToClient]
            public IProtoQuest Quest { get; }

            [SyncToClient]
            public IReadOnlyList<QuestRequirementState> RequirementStates { get; private set; }

            public void OnActiveRequirementSatisfiedStateChanged(
                ServerCharacterActiveQuestRequirement activeRequirement)
            {
                var isSatisfied = activeRequirement.QuestRequirementState.IsSatisfied;
                for (var index = 0; index < this.serverActiveRequirements.Count; index++)
                {
                    var entry = this.serverActiveRequirements[index];
                    if (entry != activeRequirement)
                    {
                        continue;
                    }

                    // found entry
                    if (isSatisfied
                        && !activeRequirement.QuestRequirement.IsReversible)
                    {
                        // remove this active requirement entry as it's satisfied and cannot be reversed
                        activeRequirement.IsActive = false;
                        this.serverActiveRequirements.RemoveAt(index);
                    }

                    this.ServerRefreshAllRequirementsSatisfied(activeRequirement.Character);
                    return;
                }

                throw new Exception("Unknown requirement satisfied: " + activeRequirement);
            }

            public void ServerCreateActiveRequirementStates(ICharacter character)
            {
                this.ServerResetActiveRequirements();
                if (this.IsCompleted)
                {
                    // the quest is finished - no active requirements required in that case
                    return;
                }

                var requirements = this.Quest.Requirements;

                this.serverActiveRequirements = new List<ServerCharacterActiveQuestRequirement>(requirements.Count);

                // refresh the requirement states
                for (byte index = 0; index < requirements.Count; index++)
                {
                    var requirement = requirements[index];
                    var requirementState = this.RequirementStates[index];
                    requirement.ServerRefreshIsSatisfied(character, requirementState);

                    if (!requirementState.IsSatisfied
                        || requirement.IsReversible)
                    {
                        // create active requirement entry
                        var questRequirement = new ServerCharacterActiveQuestRequirement(this, index, character);
                        this.serverActiveRequirements.Add(questRequirement);
                        questRequirement.IsActive = true;
                    }
                }

                this.ServerRefreshAllRequirementsSatisfied(character);
            }

            public void ServerDestroy()
            {
                this.ServerResetActiveRequirements();
            }

            public void ServerEnsureRequirementsStateIsValid()
            {
                var requirements = this.Quest.Requirements;
                var isValid = this.RequirementStates.Count == requirements.Count;
                try
                {
                    if (!isValid)
                    {
                        return;
                    }

                    // check that the requirement state types are matching
                    for (byte index = 0; index < requirements.Count; index++)
                    {
                        var requirement = requirements[index];
                        var requirementState = this.RequirementStates[index];
                        if (requirement.IsStateTypeMatch(requirementState))
                        {
                            continue;
                        }

                        isValid = false;
                        return;
                    }
                }
                finally
                {
                    if (!isValid)
                    {
                        this.ServerResetActiveRequirements();
                        this.ServerSetupRequirementStates();
                    }
                }
            }

            public void ServerRefreshAllRequirementsSatisfied(ICharacter character)
            {
                if (this.IsCompleted)
                {
                    // the quest is already finished
                    return;
                }

                var satisfied = false;
                try
                {
                    if (!this.RequirementStates.All(s => s.IsSatisfied))
                    {
                        // not all requirements are satisfied
                        return;
                    }

                    // ensure that all requirement states are updated and really satisfied
                    var requirements = this.Quest.Requirements;
                    for (byte index = 0; index < requirements.Count; index++)
                    {
                        var requirement = requirements[index];
                        var requirementState = this.RequirementStates[index];
                        requirement.ServerRefreshIsSatisfied(character, requirementState);
                    }

                    if (!this.RequirementStates.All(s => s.IsSatisfied))
                    {
                        // not all requirements are satisfied
                        return;
                    }

                    satisfied = true;
                }
                finally
                {
                    if (satisfied
                        && !this.AreAllRequirementsSatisfied)
                    {
                        //Api.Logger.Important("Quest requirements become satisfied: " + this.Quest.ShortId,
                        //                     character);
                        this.AreAllRequirementsSatisfied = true;
                    }
                    else if (!satisfied
                             && this.AreAllRequirementsSatisfied)
                    {
                        //Api.Logger.Important("Quest requirements become unsatisfied: " + this.Quest.ShortId,
                        //                     character);
                        this.AreAllRequirementsSatisfied = false;
                    }
                }
            }

            public void ServerSetCompleted()
            {
                this.ServerResetActiveRequirements();
                this.IsCompleted = true;
                this.AreAllRequirementsSatisfied = true;
                this.IsNew = false;
                this.RequirementStates = null;
            }

            private void ServerResetActiveRequirements()
            {
                if (this.serverActiveRequirements == null)
                {
                    return;
                }

                // deactivate all the active requirements
                foreach (var characterActiveQuestRequirement in this.serverActiveRequirements)
                {
                    characterActiveQuestRequirement.IsActive = false;
                }

                this.serverActiveRequirements = null;
            }

            private void ServerSetupRequirementStates()
            {
                Api.Assert(!this.IsCompleted, "Cannot create requirement states for completed quest");

                // ensure active requirements are reset
                this.ServerResetActiveRequirements();

                var requirements = this.Quest.Requirements;
                // we never modify this list but NetworkSyncList is required in order to bind its elements (QuestRequirementState) to the state owner
                var requirementStates = new NetworkSyncList<QuestRequirementState>(requirements.Count);

                foreach (var requirement in requirements)
                {
                    var state = requirement.CreateState();
                    requirementStates.Add(state);
                }

                this.RequirementStates = requirementStates;
            }
        }
    }
}